// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine.Networking;
using System;
using System.Collections;

namespace AmplifyAnimationPack
{
	public class AAPStartScreen : EditorWindow
	{
		private static readonly string ASMDefGUI = "748e6f3d2960f9341a41c502d2037679";
		private static readonly string CinemachineGUID = "4307f53044263cf4b835bd812fc161a4";
		private static readonly string ChangeLogGUID = "cb40e5633c7d4204cbd3b68d404bfcfd";


		private static readonly string AAPIconGUID = "72b71d33a4c059343b6edb8e4fcd8ac1";

		public static readonly string ChangelogURL = "http://amplify.pt/Banner/AAPchangelog.json";

		private static readonly string ManualURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Animation_Pack/Manual";
		private static readonly string AnimCatalogueURL = "http://wiki.amplify.pt/index.php?title=Unity_Products:Amplify_Animation_Pack/AnimCat";
		private static readonly string TutorialURL = "https://youtu.be/ODLSsrRk_kk";



		private static readonly string DiscordURL = "https://discordapp.com/invite/EdrVAP5";

		private static readonly string SiteURL = "http://amplify.pt/download/";
		private static readonly string AAPStoreURL = "https://assetstore.unity.com/packages/slug/207911";
		
		private static readonly GUIContent LearningResourcesTitle = new GUIContent( "Learning Resources" , "Check the online wiki for various topics about how to use AAP" );
		private static readonly GUIContent CommunityTitle = new GUIContent( "Community" , "Need help? Reach us through our discord server or the official support Unity forum" );
		private static readonly GUIContent UpdateTitle = new GUIContent( "Latest Update" , "Check the lastest additions, improvements and bug fixes done to AAP" );
		private static readonly GUIContent DependenciesTitle = new GUIContent( "Dependencies" , "List of package dependencies that need to be installed for AAP to properly work." );
		private static readonly GUIContent AAPTitle = new GUIContent( "Amplify Animation Pack" , "Are you using the latest version? Now you know" );
		private static readonly GUIContent ProjectSettingsTitle = new GUIContent( "Project Settings" , "Settings required for AAP to properly work." );


		private const string CinemachinePackageId = "com.unity.cinemachine";
		private const string TMProPackageId = "com.unity.textmeshpro";
		private const string PPSv2PackageId = "com.unity.postprocessing";

		private UnityWebRequest m_webRequest;

		private Vector2 m_scrollPosition = Vector2.zero;
		private AAPPreferences.ShowOption m_startup = AAPPreferences.ShowOption.Never;

		[NonSerialized]
		private Texture m_packageIcon = null;

		[NonSerialized]
		private Texture m_textIcon = null;

		[NonSerialized]
		private Texture m_webIcon = null;

		
		//Left Column buttons
		private GUIContent m_Manualbutton = null;
		private GUIContent m_AnimCatalogueButton = null;
		private GUIContent m_TutorialButton = null;

		//Social media buttons
		private GUIContent m_DiscordButton = null;
		private GUIContent m_ForumButton = null;

		//Depedencies buttons
		private GUIContent m_TMProButton = null;
		private GUIContent m_CinemachineButton = null;
		private GUIContent m_PPSv2Button = null;


		private GUIContent m_AAPIcon = null;
		private RenderTexture m_textIconRT;

		[NonSerialized]
		private GUIStyle m_buttonStyle = null;
		[NonSerialized]
		private GUIStyle m_buttonLeftStyle = null;
		[NonSerialized]
		private GUIStyle m_buttonRightStyle = null;
		[NonSerialized]
		private GUIStyle m_minibuttonStyle = null;
		[NonSerialized]
		private GUIStyle m_labelStyle = null;
		[NonSerialized]
		private GUIStyle m_linkStyle = null;

		private GUIStyle m_noMarginLinkStyle = null;

		private ChangeLogInfo m_changeLog;
		private bool m_infoDownloaded = false;
		private string m_newVersion = string.Empty;

		private ListRequest m_packageListRequest = null;

		private bool m_requireUpdateList = false;

		private UnityEditor.PackageManager.PackageInfo m_TMProPackageInfo = null;
		private bool m_foundTMPro = false;

		private UnityEditor.PackageManager.PackageInfo m_CinemachinePackageInfo = null;
		private bool m_foundCinemachine = false;


		private UnityEditor.PackageManager.PackageInfo m_PPSv2PackageInfo = null;
		private bool m_foundPPSv2 = false;

		[MenuItem( "Window/Amplify Animation Pack/Start Screen" , false , 1998 )]
		public static void Init()
		{
			AAPStartScreen window = (AAPStartScreen)GetWindow( typeof( AAPStartScreen ) , true , "Amplify Animation Pack Start Screen" );
			window.minSize = new Vector2( 660 , 700 );
			window.maxSize = new Vector2( 660 , 700 );
			window.Show();
		}

		void RequestPackageInfo()
		{
			if( !m_requireUpdateList )
			{
				m_requireUpdateList = true;
				m_packageListRequest = UnityEditor.PackageManager.Client.List( true );

				m_TMProPackageInfo = null;
				m_foundTMPro = false;

				m_CinemachinePackageInfo = null;
				m_foundCinemachine = false;
			}
		}

		void CheckPackageRequest()
		{
			if( m_requireUpdateList )
			{
				if( m_packageListRequest != null && m_packageListRequest.IsCompleted )
				{
					m_requireUpdateList = false;
					foreach( UnityEditor.PackageManager.PackageInfo pi in m_packageListRequest.Result )
					{
						if( pi.name.Equals( TMProPackageId ) )
						{
							m_TMProPackageInfo = pi;
							m_foundTMPro = !string.IsNullOrEmpty( pi.version ); ;
						}

						if( pi.name.Equals( CinemachinePackageId ) )
						{
							m_CinemachinePackageInfo = pi;
							m_foundCinemachine = !string.IsNullOrEmpty( pi.version );
						}

						if( pi.name.Equals( PPSv2PackageId ) )
						{
							m_PPSv2PackageInfo = pi;
							m_foundPPSv2 = !string.IsNullOrEmpty( pi.version );
						}
					}
				}
			}
		}

		private void OnEnable()
		{
			m_textIconRT = new RenderTexture( 16 , 16 , 0 );
			m_textIconRT.Create();

			m_startup = (AAPPreferences.ShowOption)EditorPrefs.GetInt( AAPPreferences.PrefStartUp , 0 );

			if( m_textIcon == null )
			{
				Texture icon = EditorGUIUtility.IconContent( "TextAsset Icon" ).image;
				var cache = RenderTexture.active;
				RenderTexture.active = m_textIconRT;
				Graphics.Blit( icon , m_textIconRT );
				RenderTexture.active = cache;
				m_textIcon = m_textIconRT;

				m_Manualbutton = new GUIContent( " Manual" , m_textIcon );
				m_AnimCatalogueButton = new GUIContent( " Animation Catalog" , m_textIcon );
				m_TutorialButton = new GUIContent( " Tutorial" , m_textIcon );

			}

			if( m_packageIcon == null )
			{
				m_packageIcon = EditorGUIUtility.IconContent( "BuildSettings.Editor.Small" ).image;
				m_TMProButton = new GUIContent( "Add TMPro" , m_packageIcon );
				m_CinemachineButton = new GUIContent( "Add Cinemachine" , m_packageIcon );
				m_PPSv2Button = new GUIContent( "Add PPSv2" , m_packageIcon );
			}

			if( m_webIcon == null )
			{
				m_webIcon = EditorGUIUtility.IconContent( "BuildSettings.Web.Small" ).image;
				m_DiscordButton = new GUIContent( " Discord" , m_webIcon );
				m_ForumButton = new GUIContent( " Unity Forum" , m_webIcon );
			}

			if( m_changeLog == null )
			{
				var changelog = AssetDatabase.LoadAssetAtPath<TextAsset>( AssetDatabase.GUIDToAssetPath( ChangeLogGUID ) );
				string lastUpdate = string.Empty;
				if( changelog != null )
				{
					lastUpdate = changelog.text;
					lastUpdate = lastUpdate.Replace( "    *" , "    \u25CB" );
					lastUpdate = lastUpdate.Replace( "* " , "\u2022 " );
				}
				m_changeLog = new ChangeLogInfo( VersionInfo.FullNumber , lastUpdate );
			}

			if( m_AAPIcon == null )
			{
				m_AAPIcon = new GUIContent( AssetDatabase.LoadAssetAtPath<Texture2D>( AssetDatabase.GUIDToAssetPath( AAPIconGUID ) ) );
			}

			if( AAPPackageManagerHelper.FinishedEvent == null )
			{
				AAPPackageManagerHelper.FinishedEvent += RequestPackageInfo;
				AAPPackageManagerHelper.FinishedEvent += StartReimportASMDef;
			}

			RequestPackageInfo();
		}

		void ReimportASMDef()
		{
			string cinemachinePath = AssetDatabase.GUIDToAssetPath( CinemachineGUID );
			if( AssetDatabase.GetMainAssetTypeAtPath( cinemachinePath ) != null )
			{
				AssetDatabase.ImportAsset( AssetDatabase.GUIDToAssetPath( ASMDefGUI ) , ImportAssetOptions.ForceUpdate );
				AssetDatabase.Refresh( ImportAssetOptions.ForceUpdate );
			}
		}

		private bool m_forceAsmdefUpdate = false;
		public bool ForceAsmdefUpdate { get { return m_forceAsmdefUpdate; } }
		void StartReimportASMDef()
		{
			m_forceAsmdefUpdate = true;
		}

		private void OnDisable()
		{
			if( m_textIconRT != null )
			{
				m_textIconRT.Release();
				DestroyImmediate( m_textIconRT );
			}
		}

		public void OnGUI()
		{
#if CINEMACHINE_PRESENT
			m_forceAsmdefUpdate = false;
#else
			if( m_forceAsmdefUpdate )
			{
				if( Event.current.type == EventType.Layout )
				{
					ReimportASMDef();
				}
			}
#endif
			CheckPackageRequest();
			if( !m_infoDownloaded )
			{
				m_infoDownloaded = true;

				StartBackgroundTask( StartRequest( ChangelogURL , () =>
				{
					var temp = ChangeLogInfo.CreateFromJSON( m_webRequest.downloadHandler.text );
					if( temp != null && temp.Version >= m_changeLog.Version )
					{
						m_changeLog = temp;
					}
					// improve this later
					int major = m_changeLog.Version / 10000;
					int minor = ( m_changeLog.Version / 1000 ) - major * 10;
					int release = ( m_changeLog.Version / 100 ) - major * 100 - minor * 10;
					int revision = ( ( m_changeLog.Version / 10 ) - major * 1000 - minor * 100 - release * 10 ) + ( m_changeLog.Version - major * 10000 - minor * 1000 - release * 100 );
					m_newVersion = major + "." + minor + "." + release + "r" + revision;
					Repaint();
				} ) );
			}

			if( m_buttonStyle == null )
			{
				m_buttonStyle = new GUIStyle( GUI.skin.button );
				m_buttonStyle.alignment = TextAnchor.MiddleLeft;
			}

			if( m_buttonLeftStyle == null )
			{
				m_buttonLeftStyle = new GUIStyle( "ButtonLeft" );
				m_buttonLeftStyle.alignment = TextAnchor.MiddleLeft;
				m_buttonLeftStyle.margin = m_buttonStyle.margin;
				m_buttonLeftStyle.margin.right = 0;
			}

			if( m_buttonRightStyle == null )
			{
				m_buttonRightStyle = new GUIStyle( "ButtonRight" );
				m_buttonRightStyle.alignment = TextAnchor.MiddleLeft;
				m_buttonRightStyle.margin = m_buttonStyle.margin;
				m_buttonRightStyle.margin.left = 0;
			}

			if( m_minibuttonStyle == null )
			{
				m_minibuttonStyle = new GUIStyle( "MiniButton" );
				m_minibuttonStyle.alignment = TextAnchor.MiddleLeft;
				m_minibuttonStyle.margin = m_buttonStyle.margin;
				m_minibuttonStyle.margin.left = 20;
				m_minibuttonStyle.normal.textColor = m_buttonStyle.normal.textColor;
				m_minibuttonStyle.hover.textColor = m_buttonStyle.hover.textColor;
			}

			if( m_labelStyle == null )
			{
				m_labelStyle = new GUIStyle( "BoldLabel" );
				m_labelStyle.margin = new RectOffset( 4 , 4 , 4 , 4 );
				m_labelStyle.padding = new RectOffset( 2 , 2 , 2 , 2 );
				m_labelStyle.fontSize = 13;
			}

			if( m_linkStyle == null )
			{
				var inv = AssetDatabase.LoadAssetAtPath<Texture2D>( AssetDatabase.GUIDToAssetPath( "1004d06b4b28f5943abdf2313a22790a" ) ); // find a better solution for transparent buttons
				m_linkStyle = new GUIStyle();
				m_linkStyle.normal.textColor = new Color( 0.2980392f , 0.4901961f , 1f );
				m_linkStyle.hover.textColor = Color.white;
				m_linkStyle.active.textColor = Color.grey;
				m_linkStyle.margin.top = 3;
				m_linkStyle.margin.bottom = 2;
				m_linkStyle.hover.background = inv;
				m_linkStyle.active.background = inv;
			}

			if( m_noMarginLinkStyle == null )
			{
				m_noMarginLinkStyle = new GUIStyle( "BoldLabel" );
				m_noMarginLinkStyle.normal.textColor = new Color( 0.2980392f , 0.4901961f , 1f );
				m_noMarginLinkStyle.hover.textColor = Color.white;
				m_noMarginLinkStyle.active.textColor = Color.grey;
				m_noMarginLinkStyle.hover.background = m_linkStyle.hover.background;
				m_noMarginLinkStyle.active.background = m_linkStyle.active.background;
			}

			EditorGUILayout.BeginHorizontal( GUIStyle.none , GUILayout.ExpandWidth( true ) );
			{
				/////////////////////////////////////////////////////////////////////////////////////
				// LEFT COLUMN
				/////////////////////////////////////////////////////////////////////////////////////
				EditorGUILayout.BeginVertical( GUILayout.Width( 175 ) );
				{
					GUILayout.Label( LearningResourcesTitle , m_labelStyle );
					if( GUILayout.Button( m_Manualbutton , m_buttonStyle ) )
						Application.OpenURL( ManualURL );

					if( GUILayout.Button( m_AnimCatalogueButton , m_buttonStyle ) )
						Application.OpenURL( AnimCatalogueURL );

					if( GUILayout.Button( m_TutorialButton , m_buttonStyle ) )
						Application.OpenURL( TutorialURL );
				}
				EditorGUILayout.EndVertical();

				/////////////////////////////////////////////////////////////////////////////////////
				// RIGHT COLUMN
				/////////////////////////////////////////////////////////////////////////////////////
				EditorGUILayout.BeginVertical( GUILayout.Width( 650 - 175 - 9 ) , GUILayout.ExpandHeight( true ) );
				{
					GUILayout.Space( 20 );

					//Discord
					if( GUILayout.Button( m_DiscordButton , GUILayout.ExpandWidth( true ) ) )
					{
						Application.OpenURL( DiscordURL );
					}


					//Dependencies
					Color bufferColor = GUI.color;
					GUILayout.Label( DependenciesTitle , m_labelStyle );
					GUILayout.BeginVertical( GUILayout.Width( 250 ) );
					{
						//TMP Pro
						GUI.color = m_foundTMPro ? Color.green : Color.red;
						if( m_foundTMPro )
						{
							GUILayout.Label( "TMPro version: " + m_TMProPackageInfo.version );
						}
						else
						{
							m_TMProPackageInfo = null;
							EditorGUILayout.BeginVertical( GUILayout.Width( 100 ) );
							{
								GUILayout.Label( "TMPro version: Not found" );
								GUI.color = bufferColor;
								if( GUILayout.Button( m_TMProButton ) )
								{
									AAPPackageManagerHelper.ImportPackage( TMProPackageId );
								}
							}
							EditorGUILayout.EndVertical();
							
						}

						//Cinemachine
						GUI.color = m_foundCinemachine ? Color.green : Color.red;
						if( m_foundCinemachine )
						{	
							GUILayout.Label( "Cinemachine version: " + m_CinemachinePackageInfo.version );
						}
						else
						{
							m_CinemachinePackageInfo = null;
							EditorGUILayout.BeginVertical(GUILayout.Width( 100 ));
							{
								GUILayout.Label( "Cinemachine version: Not found" );
								GUI.color = bufferColor;
								if( GUILayout.Button( m_CinemachineButton ) )
								{
									AAPPackageManagerHelper.ImportPackage( CinemachinePackageId );
								}
							}
							EditorGUILayout.EndVertical();
						}

						//PPSv2
						GUI.color = m_foundPPSv2 ? Color.green : Color.red;
						if( m_foundPPSv2 )
						{
							GUILayout.Label( "PPSv2 version: " + m_PPSv2PackageInfo.version );
						}
						else
						{
							m_PPSv2PackageInfo = null;
							EditorGUILayout.BeginVertical( GUILayout.Width( 100 ) );
							{
								GUILayout.Label( "PPSv2 version: Not found (Optional - Demo Scene only)" );
								GUI.color = bufferColor;
								if( GUILayout.Button( m_PPSv2Button ) )
								{
									AAPPackageManagerHelper.ImportPackage( PPSv2PackageId );
								}
							}
							EditorGUILayout.EndVertical();
						}
					}
					GUILayout.EndVertical();
					GUI.color = bufferColor;
					// Project Settings
					GUILayout.Label( ProjectSettingsTitle , m_labelStyle );
					GUILayout.BeginVertical( GUILayout.Width( 250 ) );
					{
						bool checkValue = Time.fixedDeltaTime <= 0.0165f || Time.fixedDeltaTime >= 0.0167f;
						GUI.color = checkValue ? Color.red : Color.green;
						if( checkValue )
						{
							GUILayout.Label( "Incorrect fixed timestamp found: " + Time.fixedDeltaTime );
							GUI.color = bufferColor;
							if( GUILayout.Button( "Fix" ) )
							{
								const string TimeManagerAssetPath = "ProjectSettings/TimeManager.asset";
								SerializedObject timeManagerObject = new SerializedObject( AssetDatabase.LoadAllAssetsAtPath( TimeManagerAssetPath )[ 0 ] );
								SerializedProperty fixedTimestepProperty = timeManagerObject.FindProperty( "Fixed Timestep" );
								fixedTimestepProperty.floatValue = 0.0166667f;
								timeManagerObject.ApplyModifiedProperties();
							}
						}
						else
						{
							GUILayout.Label( "Correct fixed timestamp found: " + Time.fixedDeltaTime );
						}

						GUI.color = bufferColor;
					}
					GUILayout.EndVertical();
					// Change Log
					GUILayout.Label( UpdateTitle , m_labelStyle );
					m_scrollPosition = GUILayout.BeginScrollView( m_scrollPosition , "ProgressBarBack" , GUILayout.ExpandHeight( true ) , GUILayout.ExpandWidth( true ) );
					GUILayout.Label( m_changeLog.LastUpdate , "WordWrappedMiniLabel" , GUILayout.ExpandHeight( true ) );
					GUILayout.EndScrollView();

					// Version and Links
					EditorGUILayout.BeginHorizontal( GUILayout.ExpandWidth( true ) );
					{
						EditorGUILayout.BeginVertical();
						{
							GUILayout.Label( AAPTitle , m_labelStyle );

							GUILayout.Label( "Installed Version: " + VersionInfo.StaticToString() );

							if( m_changeLog.Version > VersionInfo.FullNumber )
							{
								var cache = GUI.color;
								GUI.color = Color.red;
								GUILayout.Label( "New version available: " + m_newVersion , "BoldLabel" );
								GUI.color = cache;
							}
							else
							{
								var cache = GUI.color;
								GUI.color = Color.green;
								GUILayout.Label( "You are using the latest version" , "BoldLabel" );
								GUI.color = cache;
							}

							EditorGUILayout.BeginHorizontal();
							{
								GUILayout.Label( "Download links:" );
								if( GUILayout.Button( "Amplify" , m_linkStyle ) )
									Application.OpenURL( SiteURL );
								GUILayout.Label( "-" );
								if( GUILayout.Button( "Asset Store" , m_linkStyle ) )
									Application.OpenURL( AAPStoreURL );
							}
							EditorGUILayout.EndHorizontal();
							GUILayout.Space( 7 );
						}
						EditorGUILayout.EndVertical();

						GUILayout.FlexibleSpace();
						EditorGUILayout.BeginVertical();
						{
							GUILayout.Space( 7 );
							GUILayout.Label( m_AAPIcon );
						}
						EditorGUILayout.EndVertical();
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();


			EditorGUILayout.BeginHorizontal( "ProjectBrowserBottomBarBg" , GUILayout.ExpandWidth( true ) , GUILayout.Height( 22 ) );
			{
				GUILayout.FlexibleSpace();
				EditorGUI.BeginChangeCheck();
				var cache = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 100;
				m_startup = (AAPPreferences.ShowOption)EditorGUILayout.EnumPopup( "Show At Startup" , m_startup , GUILayout.Width( 220 ) );
				EditorGUIUtility.labelWidth = cache;
				if( EditorGUI.EndChangeCheck() )
				{
					EditorPrefs.SetInt( AAPPreferences.PrefStartUp , (int)m_startup );
				}
			}
			EditorGUILayout.EndHorizontal();

			// Find a better way to update link buttons without repainting the window
			Repaint();
		}

		IEnumerator StartRequest( string url , Action success = null )
		{
			using( m_webRequest = UnityWebRequest.Get( url ) )
			{
				yield return m_webRequest.SendWebRequest();
				while( m_webRequest.isDone == false )
					yield return null;

				if( success != null )
					success();
			}
		}

		public static void StartBackgroundTask( IEnumerator update , Action end = null )
		{
			EditorApplication.CallbackFunction closureCallback = null;

			closureCallback = () =>
			{
				try
				{
					if( update.MoveNext() == false )
					{
						if( end != null )
							end();
						EditorApplication.update -= closureCallback;
					}
				}
				catch( Exception ex )
				{
					if( end != null )
						end();
					Debug.LogException( ex );
					EditorApplication.update -= closureCallback;
				}
			};

			EditorApplication.update += closureCallback;
		}
	}

	[Serializable]
	internal class ChangeLogInfo
	{
		public int Version;
		public string LastUpdate;

		public static ChangeLogInfo CreateFromJSON( string jsonString )
		{
			if( string.IsNullOrEmpty( jsonString ) )
			{
				Debug.LogWarning( "Could not retrieve online change log for Amplify Animation Pack" );
				return null;
			}
			return JsonUtility.FromJson<ChangeLogInfo>( jsonString );
		}

		public ChangeLogInfo( int version , string lastUpdate )
		{
			Version = version;
			LastUpdate = lastUpdate;
		}
	}
}
