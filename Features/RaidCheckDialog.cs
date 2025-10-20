using System;
using Cysharp.Threading.Tasks;
using EfDEnhanced.Utils;
using UnityEngine;

namespace EfDEnhanced.Features;

/// <summary>
/// Raid进入检查对话框
/// 借鉴游戏的StrongNotification系统实现异步确认
/// 
/// 关键设计：
/// - 不管理InputManager状态，避免与MapSelectionView冲突
/// - MapSelectionView的OnOpen已调用DisableInput，我们只需监听UI输入
/// - 对话框关闭后，由Patch层模拟取消按钮，让MapSelectionView正常退出异步流程
/// - 这样确保输入状态由原生View系统统一管理，不会出现状态混乱
/// </summary>
public class RaidCheckDialog : MonoBehaviour
{
    private static RaidCheckDialog? _instance;
    private UniTaskCompletionSource<bool>? _confirmationSource;
    private bool _isShowing;
    
    public static RaidCheckDialog? Instance => _instance;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            ModLogger.Log("RaidCheckDialog", "Dialog system initialized");
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        // 订阅UI输入事件
        // 注意：使用 OnCancelEarly 而不是 OnCancel
        // 因为 MapSelectionView 作为 ActiveView 会消费掉 OnCancel 事件
        UIInputManager.OnConfirm += OnConfirm;
        UIInputManager.OnCancelEarly += OnCancel;
    }
    
    private void OnDestroy()
    {
        UIInputManager.OnConfirm -= OnConfirm;
        UIInputManager.OnCancelEarly -= OnCancel;
        
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    /// <summary>
    /// 显示对话框并等待用户确认
    /// </summary>
    /// <param name="result">检查结果</param>
    /// <returns>true=继续进入, false=取消</returns>
    public async UniTask<bool> ShowAndWaitForConfirmation(RaidCheckResult result)
    {
        try
        {
            if (_isShowing)
            {
                ModLogger.LogWarning("RaidCheckDialog", "Dialog already showing, ignoring request");
                return false;
            }
            
            _isShowing = true;
            
            // 注意：不需要调用InputManager.DisableInput()
            // 因为MapSelectionView已经在OnOpen()中调用过了
            // 重复调用会导致输入状态管理混乱
            
            // 显示通知
            string warningText = result.GetWarningText();
            ModLogger.Log("RaidCheckDialog", $"Showing warning: {warningText}");
            
            // 使用游戏的StrongNotification系统显示警告
            Duckov.StrongNotification.Push(
                "⚠ 准备检查",
                warningText + "\n\n[确认] 继续  |  [取消] 返回"
            );
            
            // 创建异步完成源
            _confirmationSource = new UniTaskCompletionSource<bool>();
            
            // 等待用户选择
            bool result_choice = await _confirmationSource.Task;
            
            ModLogger.Log("RaidCheckDialog", $"User choice: {(result_choice ? "Continue" : "Cancel")}");
            
            return result_choice;
        }
        catch (Exception ex)
        {
            ModLogger.LogError($"RaidCheckDialog.ShowAndWaitForConfirmation failed: {ex}");
            return false; // 出错时默认取消
        }
        finally
        {
            // 注意：不需要调用InputManager.ActiveInput()
            // 输入状态由MapSelectionView在Close时管理
            
            _isShowing = false;
            _confirmationSource = null;
        }
    }
    
    /// <summary>
    /// 处理确认输入（继续进入）
    /// </summary>
    private void OnConfirm(UIInputEventData eventData)
    {
        if (!_isShowing || _confirmationSource == null)
        {
            return;
        }
        
        if (eventData.Used)
        {
            return;
        }
        
        ModLogger.Log("RaidCheckDialog", "Confirm pressed");
        _confirmationSource.TrySetResult(true);
        eventData.Use();
    }
    
    /// <summary>
    /// 处理取消输入（返回）
    /// </summary>
    private void OnCancel(UIInputEventData eventData)
    {
        ModLogger.Log("RaidCheckDialog", $"OnCancel called - isShowing: {_isShowing}, source null: {_confirmationSource == null}, eventUsed: {eventData?.Used}");
        
        if (!_isShowing || _confirmationSource == null)
        {
            return;
        }
        
        if (eventData.Used)
        {
            return;
        }
        
        ModLogger.Log("RaidCheckDialog", "Cancel pressed - setting result to false");
        bool setResult = _confirmationSource.TrySetResult(false);
        ModLogger.Log("RaidCheckDialog", $"TrySetResult returned: {setResult}");
        eventData.Use();
    }
    
    /// <summary>
    /// 初始化或获取对话框实例
    /// </summary>
    public static RaidCheckDialog GetOrCreate()
    {
        if (_instance != null)
        {
            return _instance;
        }
        
        // 创建新的GameObject来承载对话框
        GameObject dialogObject = new GameObject("RaidCheckDialog");
        RaidCheckDialog dialog = dialogObject.AddComponent<RaidCheckDialog>();
        
        ModLogger.Log("RaidCheckDialog", "Created new dialog instance");
        
        return dialog;
    }
}

