using System;
using UnityEngine;

namespace FIMSpace
{
    /// <summary>
    /// FMoeglich: Class with methods which can be helpful in using Unity Console.
    /// Recommending to use some console extensions like Console Enchanced or other.
    /// </summary>
    public static class FDebug
    {
        public static void Log(string log)
        {
            Debug.Log("LOG: " + log);
        }

        public static void Log(string log, string category)
        {
            Debug.Log(MarkerColor("#1A6600") + "[" + category + "]" + EndColorMarker() + " " + log);
        }

        public static void LogRed(string log)
        {
            Debug.Log(MarkerColor("red") + log + EndColorMarker());
        }

        public static void LogOrange(string log)
        {
            Debug.Log(MarkerColor("#D1681D") + log + EndColorMarker());
        }

        public static void LogYellow(string log)
        {
            Debug.Log(MarkerColor("#E0D300") + log + EndColorMarker());
        }

        private static readonly System.Diagnostics.Stopwatch _debugWatch = new System.Diagnostics.Stopwatch();
        public static void StartMeasure()
        {
            _debugWatch.Reset();
            _debugWatch.Start();
        }

        public static void PauseMeasure()
        {
            _debugWatch.Stop();
        }

        public static void ResumeMeasure()
        {
            _debugWatch.Start();
        }

        public static void EndMeasureAndLog(string v)
        {
            _debugWatch.Stop();
            _LastMeasureMilliseconds = _debugWatch.ElapsedMilliseconds;
            _LastMeasureTicks = _debugWatch.ElapsedTicks;
            UnityEngine.Debug.Log("Measure " + v + ": " + _debugWatch.ElapsedTicks + " ticks   " + _debugWatch.ElapsedMilliseconds + "ms");
        }

        public static long EndMeasureAndGetTicks()
        {
            _debugWatch.Stop();
            _LastMeasureMilliseconds = _debugWatch.ElapsedMilliseconds;
            _LastMeasureTicks = _debugWatch.ElapsedTicks;
            return _debugWatch.ElapsedTicks;
        }

        public static long _LastMeasureMilliseconds = 0;
        public static long _LastMeasureTicks = 0;

        /// <summary>
        /// Rich text marker for color
        /// </summary>
        public static string MarkerColor(string color)
        {
            return "<color='" + color + "'>";
        }

        /// <summary>
        /// close rich text marker for color
        /// </summary>
        public static string EndColorMarker()
        {
            return "</color>";
        }


        public static void DrawBounds2D(this Bounds b, Color c, float y = 0f, float scale = 1f, float duration = 1.1f)
        {
            Vector3 fr1 = new Vector3(b.max.x, y, b.max.z) * scale;
            Vector3 br1 = new Vector3(b.max.x, y, b.min.z) * scale;
            Vector3 bl1 = new Vector3(b.min.x, y, b.min.z) * scale;
            Vector3 fl1 = new Vector3(b.min.x, y, b.max.z) * scale;
            Debug.DrawLine(fr1, br1, c, duration);
            Debug.DrawLine(br1, bl1, c, duration);
            Debug.DrawLine(br1, bl1, c, duration);
            Debug.DrawLine(bl1, fl1, c, duration);
            Debug.DrawLine(fl1, fr1, c, duration);
        }

        public static void DrawBounds3D(this Bounds b, Color c, float scale = 1f, float time = 1.01f)
        {
            Vector3 fr1 = new Vector3(b.max.x, b.min.y, b.max.z) * scale;
            Vector3 br1 = new Vector3(b.max.x, b.min.y, b.min.z) * scale;
            Vector3 bl1 = new Vector3(b.min.x, b.min.y, b.min.z) * scale;
            Vector3 fl1 = new Vector3(b.min.x, b.min.y, b.max.z) * scale;
            Debug.DrawLine(fr1, br1, c, time);
            Debug.DrawLine(br1, bl1, c, time);
            Debug.DrawLine(br1, bl1, c, time);
            Debug.DrawLine(bl1, fl1, c, time);
            Debug.DrawLine(fl1, fr1, c, time);

            Vector3 fr = new Vector3(b.max.x, b.max.y, b.max.z) * scale;
            Vector3 br = new Vector3(b.max.x, b.max.y, b.min.z) * scale;
            Vector3 bl = new Vector3(b.min.x, b.max.y, b.min.z) * scale;
            Vector3 fl = new Vector3(b.min.x, b.max.y, b.max.z) * scale;
            Debug.DrawLine(fr, br, c, time);
            Debug.DrawLine(br, bl, c, time);
            Debug.DrawLine(br, bl, c, time);
            Debug.DrawLine(bl, fl, c, time);
            Debug.DrawLine(fl, fr, c, time);

            Debug.DrawLine(fr1, fr1, c,time);
            Debug.DrawLine(br, br1, c, time);
            Debug.DrawLine(bl1, bl, c, time);
            Debug.DrawLine(fl1, fl, c, time);
        }


    }
}