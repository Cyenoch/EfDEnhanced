using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using Duckov.Modding;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETStandard,Version=v2.1", FrameworkDisplayName = ".NET Standard 2.1")]
[assembly: AssemblyCompany("CFKillFeedback")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0+9fe43583d2b026949eeb42f4f09cab9b0bbcb3ff")]
[assembly: AssemblyProduct("CFKillFeedback")]
[assembly: AssemblyTitle("CFKillFeedback")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Parameter | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableAttribute : Attribute
	{
		public readonly byte[] NullableFlags;

		public NullableAttribute(byte P_0)
		{
			NullableFlags = new byte[1] { P_0 };
		}

		public NullableAttribute(byte[] P_0)
		{
			NullableFlags = P_0;
		}
	}
	[CompilerGenerated]
	[Microsoft.CodeAnalysis.Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class NullableContextAttribute : Attribute
	{
		public readonly byte Flag;

		public NullableContextAttribute(byte P_0)
		{
			Flag = P_0;
		}
	}
}
namespace CFKillFeedback
{
	public class ModBehaviour : ModBehaviour
	{
		public static bool Loaded = false;

		public static Dictionary<string, object> DefaultConfig = new Dictionary<string, object>();

		public static float IconStayAlpha = 0.75f;

		public static float IconDropTime = 0.1f;

		public static float IconStayTime = 1f;

		public static float IconFadeoutTime = 1.25f;

		public static Vector3 IconSizeDrop = new Vector3(4f, 4f);

		public static Vector3 IconSizeStay = new Vector3(2f, 2f);

		public static Vector2 IconSizeBaseMulti = new Vector2(960f, 540f);

		public static Vector3 IconPosPrctDrop = new Vector3(0.5f, 0.1f);

		public static Vector3 IconPosPrctStay = new Vector3(0.5f, 0.2f);

		public static float IconShakeLength = 0.1f;

		public static float IconShakeModSecond = 0.05f;

		public const float ComboSeconds = 8f;

		public static ModBehaviour? Instance;

		public static readonly string[] IconNames = new string[10] { "kill", "kill2", "kill3", "kill4", "kill5", "kill6", "headshot", "headshot_gold", "grenade_kill", "melee_kill" };

		public static readonly string[] AudioNames = new string[12]
		{
			"kill", "kill2", "kill3", "kill4", "kill5", "kill6", "kill7", "kill8", "headshot", "grenade_kill",
			"melee_kill", "death"
		};

		public static Dictionary<string, Texture2D> KillFeedbackIcons = new Dictionary<string, Texture2D>();

		public static Dictionary<string, Sound> KillFeedbackAudios_FMOD = new Dictionary<string, Sound>();

		internal static Image? ui_image;

		internal static RectTransform? ui_transform;

		internal static CanvasGroup? ui_canvasgroup;

		public static float volume = 1f;

		public static bool simple_sfx = false;

		public static bool disable_icon = false;

		public static float last_kill_time = 0f;

		public static int combo_count = 0;

		private void Update()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_0223: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0262: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0138: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01df: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Unknown result type (might be due to invalid IL or missing references)
			float num = Time.time - last_kill_time;
			Vector3 position = default(Vector3);
			if ((Object)(object)ui_canvasgroup != (Object)null && (Object)(object)ui_transform != (Object)null)
			{
				Vector2 val = Vector2.op_Implicit(((Transform)ui_transform).parent.position * 2f);
				if (num < IconDropTime)
				{
					float num2 = Math.Clamp(num / IconDropTime, 0f, 1f);
					ui_canvasgroup.alpha = num2 * IconStayAlpha;
					((Transform)ui_transform).localScale = Vector3.Lerp(IconSizeDrop, IconSizeStay, num2) * Math.Min(((Transform)ui_transform).parent.position.x / IconSizeBaseMulti.x, ((Transform)ui_transform).parent.position.x / IconSizeBaseMulti.y);
					Vector3 val2 = Vector3.Lerp(IconPosPrctDrop, IconPosPrctStay, num2);
					((Vector3)(ref position))..ctor(val2.x * val.x, val2.y * val.y);
				}
				else if (num > IconDropTime + IconStayTime)
				{
					((Transform)ui_transform).localScale = IconSizeStay * Math.Min(((Transform)ui_transform).parent.position.x / IconSizeBaseMulti.x, ((Transform)ui_transform).parent.position.x / IconSizeBaseMulti.y);
					ui_canvasgroup.alpha = (1f - Math.Clamp((num - IconDropTime - IconStayTime) / IconFadeoutTime, 0f, 1f)) * IconStayAlpha;
					((Vector3)(ref position))..ctor(IconPosPrctStay.x * val.x, IconPosPrctStay.y * val.y);
				}
				else
				{
					((Transform)ui_transform).localScale = IconSizeStay * Math.Min(((Transform)ui_transform).parent.position.x / IconSizeBaseMulti.x, ((Transform)ui_transform).parent.position.x / IconSizeBaseMulti.y);
					ui_canvasgroup.alpha = IconStayAlpha;
					((Vector3)(ref position))..ctor(IconPosPrctStay.x * val.x, IconPosPrctStay.y * val.y);
				}
				((Transform)ui_transform).position = position;
				if (disable_icon)
				{
					ui_canvasgroup.alpha = 0f;
				}
			}
		}

		public void OnDead(Health health, DamageInfo damageInfo)
		{
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)health == (Object)null)
			{
				return;
			}
			if (health.IsMainCharacterHealth)
			{
				if (KillFeedbackAudios_FMOD.ContainsKey("death"))
				{
					Bus bus = RuntimeManager.GetBus("bus:/Master/SFX");
					ChannelGroup val = default(ChannelGroup);
					((Bus)(ref bus)).getChannelGroup(ref val);
					Channel val2 = default(Channel);
					System coreSystem = RuntimeManager.CoreSystem;
					((System)(ref coreSystem)).playSound(KillFeedbackAudios_FMOD["death"], val, false, ref val2);
				}
			}
			else if ((int)damageInfo.fromCharacter.Team == 0)
			{
				bool headshot = damageInfo.crit > 0;
				bool melee = (Object)(object)damageInfo.fromCharacter.GetMeleeWeapon() != (Object)null;
				bool isExplosion = damageInfo.isExplosion;
				bool goldheadshot = damageInfo.finalDamage >= health.MaxHealth * 0.9f;
				PlayKill(headshot, goldheadshot, melee, isExplosion);
			}
		}

		public void PlayKill(bool headshot, bool goldheadshot, bool melee, bool explosion)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_011a: Unknown result type (might be due to invalid IL or missing references)
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_013f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0144: Unknown result type (might be due to invalid IL or missing references)
			//IL_0164: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			//IL_0189: Unknown result type (might be due to invalid IL or missing references)
			//IL_018e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_027e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Unknown result type (might be due to invalid IL or missing references)
			//IL_0289: Unknown result type (might be due to invalid IL or missing references)
			//IL_028d: Unknown result type (might be due to invalid IL or missing references)
			//IL_028e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0292: Unknown result type (might be due to invalid IL or missing references)
			//IL_029f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0268: Unknown result type (might be due to invalid IL or missing references)
			//IL_026d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0256: Unknown result type (might be due to invalid IL or missing references)
			//IL_025b: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)ui_transform == (Object)null)
			{
				CreateUI();
			}
			UpdateCombo();
			Texture2D val = null;
			Sound val2 = default(Sound);
			if (combo_count <= 1)
			{
				val = KillFeedbackIcons["kill"];
				val2 = KillFeedbackAudios_FMOD["kill"];
				if (headshot)
				{
					val2 = KillFeedbackAudios_FMOD["headshot"];
				}
				if (goldheadshot)
				{
					val2 = KillFeedbackAudios_FMOD["headshot"];
				}
				if (melee)
				{
					val2 = KillFeedbackAudios_FMOD["melee_kill"];
				}
				if (explosion)
				{
					val2 = KillFeedbackAudios_FMOD["grenade_kill"];
				}
			}
			else if (combo_count <= 8)
			{
				switch (combo_count)
				{
				case 2:
					val = KillFeedbackIcons["kill2"];
					val2 = KillFeedbackAudios_FMOD["kill2"];
					break;
				case 3:
					val = KillFeedbackIcons["kill3"];
					val2 = KillFeedbackAudios_FMOD["kill3"];
					break;
				case 4:
					val = KillFeedbackIcons["kill4"];
					val2 = KillFeedbackAudios_FMOD["kill4"];
					break;
				case 5:
					val = KillFeedbackIcons["kill5"];
					val2 = KillFeedbackAudios_FMOD["kill5"];
					break;
				case 6:
					val = KillFeedbackIcons["kill6"];
					val2 = KillFeedbackAudios_FMOD["kill6"];
					break;
				case 7:
					val = KillFeedbackIcons["kill6"];
					val2 = KillFeedbackAudios_FMOD["kill7"];
					break;
				case 8:
					val = KillFeedbackIcons["kill6"];
					val2 = KillFeedbackAudios_FMOD["kill8"];
					break;
				}
			}
			else
			{
				val = KillFeedbackIcons["kill6"];
				val2 = KillFeedbackAudios_FMOD["kill"];
			}
			if (headshot)
			{
				val = KillFeedbackIcons["headshot"];
			}
			if (goldheadshot)
			{
				val = KillFeedbackIcons["headshot_gold"];
			}
			if (melee)
			{
				val = KillFeedbackIcons["melee_kill"];
			}
			if (explosion)
			{
				val = KillFeedbackIcons["grenade_kill"];
			}
			if (simple_sfx)
			{
				val2 = ((!headshot) ? KillFeedbackAudios_FMOD["kill"] : KillFeedbackAudios_FMOD["headshot"]);
			}
			Bus bus = RuntimeManager.GetBus("bus:/Master/SFX");
			ChannelGroup val3 = default(ChannelGroup);
			((Bus)(ref bus)).getChannelGroup(ref val3);
			System coreSystem = RuntimeManager.CoreSystem;
			Channel val4 = default(Channel);
			((System)(ref coreSystem)).playSound(val2, val3, false, ref val4);
			((Channel)(ref val4)).setVolume(volume);
			if ((Object)(object)ui_image != (Object)null && (Object)(object)val != (Object)null)
			{
				ui_image.sprite = Sprite.Create(val, new Rect(0f, 0f, 512f, 512f), new Vector2(256f, 256f));
			}
		}

		public static void UpdateCombo()
		{
			float time = Time.time;
			if (time - last_kill_time > 8f || time - last_kill_time <= 0f)
			{
				combo_count = 1;
			}
			else
			{
				combo_count++;
			}
			last_kill_time = time;
		}

		private void Awake()
		{
			DefaultConfig.TryAdd("volume", 1f);
			DefaultConfig.TryAdd("simple_sfx", false);
			DefaultConfig.TryAdd("disable_icon", false);
			Instance = this;
			if (!Loaded)
			{
				if (LoadRes())
				{
					Debug.Log((object)"CFKillFeedback: 已载入/Loaded");
					Loaded = true;
				}
				else
				{
					Debug.LogError((object)"CFKillFeedback: 载入资源时出现问题/Something wrong when loading resources");
				}
			}
		}

		private void OnEnable()
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Invalid comparison between Unknown and I4
			//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Invalid comparison between Unknown and I4
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Invalid comparison between Unknown and I4
			Health.OnDead += OnDead;
			string path = Path.Combine(Application.streamingAssetsPath, "CFKillFeedback.cfg");
			if (File.Exists(path))
			{
				JObject val = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path));
				if (val != null)
				{
					foreach (JProperty item in val.Properties())
					{
						if (item.Name == "volume" && (int)item.Value.Type == 7)
						{
							volume = (float)item.Value;
						}
						else if (item.Name == "simple_sfx" && (int)item.Value.Type == 9)
						{
							simple_sfx = (bool)item.Value;
						}
						else if (item.Name == "disable_icon" && (int)item.Value.Type == 9)
						{
							disable_icon = (bool)item.Value;
						}
					}
					return;
				}
				Debug.LogError((object)"CFKillFeedback: 读取配置文件时出错/Failed to read config file");
			}
			else
			{
				File.WriteAllText(path, JsonConvert.SerializeObject((object)DefaultConfig, (Formatting)1));
			}
		}

		private void OnDisable()
		{
			Health.OnDead -= OnDead;
		}

		private void OnDestroy()
		{
			if ((Object)(object)ui_transform != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)ui_transform).gameObject);
			}
		}

		public unsafe bool LoadRes()
		{
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Expected O, but got Unknown
			//IL_0149: Unknown result type (might be due to invalid IL or missing references)
			//IL_014e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0157: Unknown result type (might be due to invalid IL or missing references)
			//IL_015c: Unknown result type (might be due to invalid IL or missing references)
			//IL_015e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0169: Unknown result type (might be due to invalid IL or missing references)
			Debug.Log((object)"CFKillFeedback: 开始加载资源/Starting loading resources");
			bool flag = true;
			string dllDirectory = Utils.GetDllDirectory();
			Debug.Log((object)("CFKillFeedback: Absolute path = " + dllDirectory));
			Debug.Log((object)"CFKillFeedback: 正在遍历图标名称列表/Foreaching IconNames list");
			string[] iconNames = IconNames;
			foreach (string text in iconNames)
			{
				string text2 = Path.Combine(dllDirectory, text + ".png");
				Debug.Log((object)("CFKillFeedback: Now path is " + text2));
				if (!File.Exists(text2))
				{
					Debug.LogError((object)("CFKillFeedback: 文件不存在 = " + text2));
					flag = false;
					continue;
				}
				byte[] array = File.ReadAllBytes(text2);
				Texture2D val = new Texture2D(256, 256);
				if (ImageConversion.LoadImage(val, array))
				{
					KillFeedbackIcons.TryAdd(text, val);
					flag = flag;
					Debug.Log((object)("CFKillFeedback: 纹理加载成功 = " + text2));
				}
				else
				{
					flag = false;
					Debug.LogError((object)("CFKillFeedback: 加载纹理失败/Failed to load texture = " + text2));
				}
			}
			Debug.Log((object)"CFKillFeedback: 正在遍历音频名称列表/Foreaching AudioNames list");
			iconNames = AudioNames;
			Sound value = default(Sound);
			foreach (string text3 in iconNames)
			{
				string text4 = Path.Combine(dllDirectory, text3 + ".wav");
				Debug.Log((object)("CFKillFeedback: Now path is " + text4));
				if (!File.Exists(text4))
				{
					Debug.LogError((object)("CFKillFeedback: 文件不存在 = " + text4));
					flag = false;
					continue;
				}
				System coreSystem = RuntimeManager.CoreSystem;
				RESULT val2 = ((System)(ref coreSystem)).createSound(text4, (MODE)1, ref value);
				if ((int)val2 == 0)
				{
					KillFeedbackAudios_FMOD.TryAdd(text3, value);
					flag = flag;
					Debug.Log((object)("CFKillFeedback: 成功加载音频 = " + text4));
				}
				else
				{
					Debug.LogError((object)("CFKillFeedback: 加载音频时出错 = " + ((object)(*(RESULT*)(&val2))/*cast due to .constrained prefix*/).ToString()));
					flag = false;
				}
			}
			return flag;
		}

		public void CreateUI()
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			HUDManager val = Object.FindObjectOfType<HUDManager>();
			if (!((Object)(object)val == (Object)null))
			{
				GameObject val2 = new GameObject("CFKillFeedbackUI");
				ui_transform = val2.AddComponent<RectTransform>();
				ui_image = val2.AddComponent<Image>();
				ui_image.preserveAspect = true;
				ui_canvasgroup = val2.AddComponent<CanvasGroup>();
				((Transform)ui_transform).SetParent(((Component)val).transform);
				Debug.Log((object)"CFKillFeedback: 已创建UI/UI created");
			}
		}
	}
	public class Utils
	{
		public static string GetDllDirectory()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}
	}
}
