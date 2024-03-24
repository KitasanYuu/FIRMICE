using System;
using System.Collections.Generic;
using Kinemation.Recoilly.Runtime;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Kinemation.Recoilly
{
    [Serializable]
    public enum FireMode
    {
        Semi,
        Burst,
        Auto
    }

    [Serializable]
    public struct VectorCurve
    {
        public AnimationCurve x;
        public AnimationCurve y;
        public AnimationCurve z;

        public float GetLastTime()
        {
            float maxTime = -1f;

            float curveTime = RecoilCurves.GetMaxTime(x);
            maxTime = curveTime > maxTime ? curveTime : maxTime;

            curveTime = RecoilCurves.GetMaxTime(y);
            maxTime = curveTime > maxTime ? curveTime : maxTime;

            curveTime = RecoilCurves.GetMaxTime(z);
            maxTime = curveTime > maxTime ? curveTime : maxTime;

            return maxTime;
        }

        public Vector3 Evaluate(float time)
        {
            return new Vector3(x.Evaluate(time), y.Evaluate(time), z.Evaluate(time));
        }

        public bool IsValid()
        {
            return x != null && y != null && z != null;
        }
    }

    [Serializable]
    public struct RecoilCurves
    {
        public VectorCurve semiRotCurve;
        public VectorCurve semiLocCurve;
        public VectorCurve autoRotCurve;
        public VectorCurve autoLocCurve;

        private List<AnimationCurve> _curves;
        public static float GetMaxTime(AnimationCurve curve)
        {
            if (curve == null || curve.length == 0)
            {
                Debug.LogError("The provided AnimationCurve is null or empty.");
                return 0f; // 返回一个合理的默认值或进行其他错误处理
            }

            return curve[curve.length - 1].time;
        }

    }

    public struct StartRest
    {
        public StartRest(bool x, bool y, bool z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public bool x;
        public bool y;
        public bool z;
    }

    public delegate bool ConditionDelegate();
    public delegate void PlayDelegate();
    public delegate void StopDelegate();
    public struct AnimState
    {
        public ConditionDelegate checkCondition;
        public PlayDelegate onPlay;
        public StopDelegate onStop;
    }

    public class RecoilAnimation : MonoBehaviour
    {
        public Vector3 OutRot { get; private set; }
        public Vector3 OutLoc { get; private set; }

        public bool isAiming;

        private RecoilAnimData _recoilData;
        private float _fireRate;
        public FireMode fireMode;
        private List<AnimState> _stateMachine;
        private int _stateIndex;

        private Vector3 _targetRot;
        private Vector3 _targetLoc;

        private VectorCurve _tempRotCurve;
        private VectorCurve _tempLocCurve;

        private Vector3 _startValRot;
        private Vector3 _startValLoc;

        private StartRest _canRestRot;
        private StartRest _canRestLoc;

        private Vector3 _rawRotOut;
        private Vector3 _rawLocOut;

        private Vector3 _smoothRotOut;
        private Vector3 _smoothLocOut;

        private Vector2 _noiseTarget;
        private Vector2 _noiseOut;

        private float _pushTarget;
        private float _pushOut;

        private float _lastFrameTime;
        private float _playBack;
        private float _lastTimeShot;

        private bool _isPlaying;
        private bool _isLooping;
        private bool _enableSmoothing;

        public void Init(RecoilAnimData data, float fireRate)
        {
            _recoilData = data;

            OutRot = Vector3.zero;
            OutLoc = Vector3.zero;

            _fireRate = fireRate;

            _targetRot = Vector3.zero;
            _targetLoc = Vector3.zero;

            _pushTarget = 0f;
            _noiseTarget = Vector2.zero;

            SetupStateMachine();
        }

        public void Play()
        {
            //Iterate through each transition, if true execute
            for (int i = 0; i < _stateMachine.Count; i++)
            {
                if (_stateMachine[i].checkCondition.Invoke())
                {
                    _stateIndex = i;
                    break;
                }
            }

            _stateMachine[_stateIndex].onPlay.Invoke();
            _lastTimeShot = Time.unscaledTime;
        }

        public void Stop()
        {
            _stateMachine[_stateIndex].onStop.Invoke();
            _isLooping = false;
        }

        private void Update()
        {
            if (_isPlaying)
            {
                UpdateSolver();
                UpdateTimeline();
            }
            ApplySmoothing();

            Vector3 finalLoc = _smoothLocOut;

            ApplyNoise(ref finalLoc);
            ApplyPushback(ref finalLoc);

            OutRot = _smoothRotOut;
            OutLoc = finalLoc;
        }

        private void CalculateTargetData()
        {
            float pitch = Random.Range(_recoilData.pitch.x, _recoilData.pitch.y);
            float yawMin = Random.Range(_recoilData.yaw.x, _recoilData.yaw.y);
            float yawMax = Random.Range(_recoilData.yaw.z, _recoilData.yaw.w);

            float yaw = Random.value >= 0.5f ? yawMax : yawMin;

            float rollMin = Random.Range(_recoilData.roll.x, _recoilData.roll.y);
            float rollMax = Random.Range(_recoilData.roll.z, _recoilData.roll.w);

            float roll = Random.value >= 0.5f ? rollMax : rollMin;

            roll = _targetRot.z * roll > 0f && _recoilData.smoothRoll ? -roll : roll;

            float kick = Random.Range(_recoilData.kickback.x, _recoilData.kickback.y);
            float kickRight = Random.Range(_recoilData.kickRight.x, _recoilData.kickRight.y);
            float kickUp = Random.Range(_recoilData.kickUp.x, _recoilData.kickUp.y);

            _noiseTarget.x += Random.Range(_recoilData.noiseX.x, _recoilData.noiseX.y);
            _noiseTarget.y += Random.Range(_recoilData.noiseY.x, _recoilData.noiseY.y);

            _noiseTarget.x *= isAiming ? _recoilData.noiseScalar : 1f;
            _noiseTarget.y *= isAiming ? _recoilData.noiseScalar : 1f;

            pitch *= isAiming ? _recoilData.aimRot.x : 1f;
            yaw *= isAiming ? _recoilData.aimRot.y : 1f;
            roll *= isAiming ? _recoilData.aimRot.z : 1f;

            kick *= isAiming ? _recoilData.aimLoc.z : 1f;
            kickRight *= isAiming ? _recoilData.aimLoc.x : 1f;
            kickUp *= isAiming ? _recoilData.aimLoc.y : 1f;

            _targetRot = new Vector3(pitch, yaw, roll);
            _targetLoc = new Vector3(kickRight, kickUp, kick);
        }

        private void UpdateTimeline()
        {
            _playBack += Time.deltaTime * _recoilData.playRate;
            _playBack = Mathf.Clamp(_playBack, 0f, _lastFrameTime);

            // Stop updating if the end is reached
            if (Mathf.Approximately(_playBack, _lastFrameTime))
            {
                if (_isLooping)
                {
                    _playBack = 0f;
                    _isPlaying = true;
                }
                else
                {
                    _isPlaying = false;
                    _playBack = 0f;
                }
            }
        }

        private void UpdateSolver()
        {
            if (Mathf.Approximately(_playBack, 0f))
            {
                CalculateTargetData();
            }

            // Current playback position
            float lastPlayback = _playBack - Time.deltaTime * _recoilData.playRate;
            lastPlayback = Mathf.Max(lastPlayback, 0f);

            Vector3 alpha = _tempRotCurve.Evaluate(_playBack);
            Vector3 lastAlpha = _tempRotCurve.Evaluate(lastPlayback);

            Vector3 output = Vector3.zero;

            output.x = Mathf.LerpUnclamped(
                CorrectStart(ref lastAlpha.x, alpha.x, ref _canRestRot.x, ref _startValRot.x),
                _targetRot.x, alpha.x);

            output.y = Mathf.LerpUnclamped(
                CorrectStart(ref lastAlpha.y, alpha.y, ref _canRestRot.y, ref _startValRot.y),
                _targetRot.y, alpha.y);

            output.z = Mathf.LerpUnclamped(
                CorrectStart(ref lastAlpha.z, alpha.z, ref _canRestRot.z, ref _startValRot.z),
                _targetRot.z, alpha.z);

            _rawRotOut = output;

            alpha = _tempLocCurve.Evaluate(_playBack);
            lastAlpha = _tempLocCurve.Evaluate(lastPlayback);

            output.x = Mathf.LerpUnclamped(
                CorrectStart(ref lastAlpha.x, alpha.x, ref _canRestLoc.x, ref _startValLoc.x),
                _targetLoc.x, alpha.x);

            output.y = Mathf.LerpUnclamped(
                CorrectStart(ref lastAlpha.y, alpha.y, ref _canRestLoc.y, ref _startValLoc.y),
                _targetLoc.y, alpha.y);

            output.z = Mathf.LerpUnclamped(
                CorrectStart(ref lastAlpha.z, alpha.z, ref _canRestLoc.z, ref _startValLoc.z),
                _targetLoc.z, alpha.z);

            _rawLocOut = output;
        }

        private void ApplySmoothing()
        {
            if (_enableSmoothing)
            {
                Vector3 lerped = _smoothRotOut;

                Vector3 smooth = _recoilData.smoothRot;

                Func<float, float, float, float, float> Interp = (a, b, speed, scale) =>
                {
                    scale = Mathf.Approximately(scale, 0f) ? 1f : scale;
                    return Mathf.Approximately(speed, 0f) ? b * scale : AnimToolkitLib.Glerp(a, b * scale, speed);
                };

                lerped.x = Interp(_smoothRotOut.x, _rawRotOut.x, smooth.x, _recoilData.extraRot.x);
                lerped.y = Interp(_smoothRotOut.y, _rawRotOut.y, smooth.y, _recoilData.extraRot.y);
                lerped.z = Interp(_smoothRotOut.z, _rawRotOut.z, smooth.z, _recoilData.extraRot.z);
                _smoothRotOut = lerped;

                lerped = _smoothLocOut;
                smooth = _recoilData.smoothLoc;

                lerped.x = Interp(_smoothLocOut.x, _rawLocOut.x, smooth.x, _recoilData.extraLoc.x);
                lerped.y = Interp(_smoothLocOut.y, _rawLocOut.y, smooth.y, _recoilData.extraLoc.y);
                lerped.z = Interp(_smoothLocOut.z, _rawLocOut.z, smooth.z, _recoilData.extraLoc.z);

                _smoothLocOut = lerped;
            }
            else
            {
                _smoothRotOut = _rawRotOut;
                _smoothLocOut = _rawLocOut;
            }
        }

        private void ApplyNoise(ref Vector3 finalized)
        {
            if (_recoilData == null)
            {
                //Debug.LogError("RecoilData is not initialized.");
                return;
            }

            _noiseTarget.x = AnimToolkitLib.Glerp(_noiseTarget.x, 0f, _recoilData.noiseDamp.x);
            _noiseTarget.y = AnimToolkitLib.Glerp(_noiseTarget.y, 0f, _recoilData.noiseDamp.y);

            _noiseOut.x = AnimToolkitLib.Glerp(_noiseOut.x, _noiseTarget.x, _recoilData.noiseAccel.x);
            _noiseOut.y = AnimToolkitLib.Glerp(_noiseOut.y, _noiseTarget.y, _recoilData.noiseAccel.y);

            finalized += new Vector3(_noiseOut.x, _noiseOut.y, 0f);
        }

        private void ApplyPushback(ref Vector3 finalized)
        {
            if (_recoilData == null)
            {
                //Debug.LogError("RecoilData is not initialized.");
                return;
            }

            _pushTarget = AnimToolkitLib.Glerp(_pushTarget, 0f, _recoilData.pushDamp);
            _pushOut = AnimToolkitLib.Glerp(_pushOut, _pushTarget, _recoilData.pushAccel);

            finalized += new Vector3(0f, 0f, _pushOut);
        }

        private float CorrectStart(ref float last, float current, ref bool bStartRest, ref float startVal)
        {
            if (Mathf.Abs(last) > Mathf.Abs(current) && bStartRest && !_isLooping)
            {
                startVal = 0f;
                bStartRest = false;
            }

            last = current;

            return startVal;
        }

        private void SetupStateMachine()
        {
            _stateMachine ??= new List<AnimState>();

            AnimState semiState;
            AnimState autoState;

            semiState.checkCondition = () =>
            {
                float timerError = (60f / _fireRate) / Time.deltaTime + 1;
                timerError *= Time.deltaTime;

                if (_enableSmoothing && !_isLooping)
                {
                    _enableSmoothing = false;
                }

                return GetDelta() > timerError + 0.01f && !_isLooping || fireMode == FireMode.Semi;
            };

            semiState.onPlay = () =>
            {
                SetupTransition(_smoothRotOut, _smoothLocOut, _recoilData.recoilCurves.semiRotCurve,
                    _recoilData.recoilCurves.semiLocCurve);
            };

            semiState.onStop = () =>
            {
                //Intended to be empty
            };

            autoState.checkCondition = () => true;

            autoState.onPlay = () =>
            {
                if (_isLooping)
                {
                    return;
                }

                var curves = _recoilData.recoilCurves;
                bool bCurvesValid = curves.autoRotCurve.IsValid() && curves.autoLocCurve.IsValid();

                _enableSmoothing = bCurvesValid;
                float correction = 60f / _fireRate;

                if (bCurvesValid)
                {
                    CorrectAlpha(curves.autoRotCurve, curves.autoLocCurve, correction);
                    SetupTransition(_startValRot, _startValLoc, curves.autoRotCurve, curves.autoLocCurve);
                }
                else if (curves.autoRotCurve.IsValid() && curves.autoLocCurve.IsValid())
                {
                    CorrectAlpha(curves.semiRotCurve, curves.semiLocCurve, correction);
                    SetupTransition(_startValRot, _startValLoc, curves.semiRotCurve, curves.semiLocCurve);
                }

                _pushTarget = _recoilData.pushAmount;

                _lastFrameTime = correction;
                _isLooping = true;
            };

            autoState.onStop = () =>
            {
                if (!_isLooping)
                {
                    return;
                }

                float tempRot = _tempRotCurve.GetLastTime();
                float tempLoc = _tempLocCurve.GetLastTime();
                _lastFrameTime = tempRot > tempLoc ? tempRot : tempLoc;
                _isPlaying = true;
            };

            _stateMachine.Add(semiState);
            _stateMachine.Add(autoState);
        }

        private void SetupTransition(Vector3 startRot, Vector3 startLoc, VectorCurve rot, VectorCurve loc)
        {
            if (!rot.IsValid() || !loc.IsValid())
            {
                Debug.Log("RecoilAnimation: Rot or Loc curve is nullptr");
                return;
            }

            _startValRot = startRot;
            _startValLoc = startLoc;

            _canRestRot = _canRestLoc = new StartRest(true, true, true);

            _tempRotCurve = rot;
            _tempLocCurve = loc;

            _lastFrameTime = rot.GetLastTime() > loc.GetLastTime() ? rot.GetLastTime() : loc.GetLastTime();

            PlayFromStart();
        }

        private void CorrectAlpha(VectorCurve rot, VectorCurve loc, float time)
        {
            Vector3 curveAlpha = rot.Evaluate(time);

            _startValRot.x = Mathf.LerpUnclamped(_startValRot.x, _targetRot.x, curveAlpha.x);
            _startValRot.y = Mathf.LerpUnclamped(_startValRot.y, _targetRot.y, curveAlpha.y);
            _startValRot.z = Mathf.LerpUnclamped(_startValRot.z, _targetRot.z, curveAlpha.z);

            curveAlpha = loc.Evaluate(time);

            _startValLoc.x = Mathf.LerpUnclamped(_startValLoc.x, _targetLoc.x, curveAlpha.x);
            _startValLoc.y = Mathf.LerpUnclamped(_startValLoc.y, _targetLoc.y, curveAlpha.y);
            _startValLoc.z = Mathf.LerpUnclamped(_startValLoc.z, _targetLoc.z, curveAlpha.z);
        }

        private void PlayFromStart()
        {
            _playBack = 0f;
            _isPlaying = true;
        }

        private float GetDelta()
        {
            return Time.unscaledTime - _lastTimeShot;
        }
    }
}