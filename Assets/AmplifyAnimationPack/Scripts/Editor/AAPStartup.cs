// Amplify Animation Pack - Third-Person Character Controller
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>


using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;

namespace AmplifyAnimationPack
{
	[InitializeOnLoad]
	public static class AAPStartup
	{
		static AAPStartup()
		{
			EditorApplication.update += Update;
		}

		static void Update()
		{
			EditorApplication.update -= Update;

			if( !EditorApplication.isPlayingOrWillChangePlaymode )
			{
				AAPPreferences.ShowOption show = AAPPreferences.ShowOption.Never;
				if( !EditorPrefs.HasKey( AAPPreferences.PrefStartUp ) )
				{
					show = AAPPreferences.ShowOption.Always;
					EditorPrefs.SetInt( AAPPreferences.PrefStartUp , 0 );
				}
				else
				{
					if( Time.realtimeSinceStartup < 10 )
					{
						show = (AAPPreferences.ShowOption)EditorPrefs.GetInt( AAPPreferences.PrefStartUp , 0 );
						// check version here
						if( show == AAPPreferences.ShowOption.OnNewVersion )
						{
							AAPStartScreen.StartBackgroundTask( StartRequest( AAPStartScreen.ChangelogURL , () =>
							{
								var changeLog = ChangeLogInfo.CreateFromJSON( www.downloadHandler.text );
								if( changeLog != null )
								{
									if( changeLog.Version > VersionInfo.FullNumber )
										AAPStartScreen.Init();
								}
							} ) );
						}
					}
				}

				if( show == AAPPreferences.ShowOption.Always )
					AAPStartScreen.Init();
			}
		}

		static UnityWebRequest www;

		static IEnumerator StartRequest( string url , Action success = null )
		{
			using( www = UnityWebRequest.Get( url ) )
			{
#if UNITY_2017_2_OR_NEWER
				yield return www.SendWebRequest();
#else
				yield return www.Send();
#endif

				while( www.isDone == false )
					yield return null;

				if( success != null )
					success();
			}
		}

	}
}
