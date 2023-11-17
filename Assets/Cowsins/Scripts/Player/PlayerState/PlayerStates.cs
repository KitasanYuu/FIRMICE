using UnityEngine;
namespace cowsins {
public class PlayerStates : MonoBehaviour
{
    PlayerBaseState _currentState;
    PlayerStateFactory _states; 

    public PlayerBaseState CurrentState { get { return _currentState;  } set{ _currentState = value;  } }
    public PlayerStateFactory _States { get { return _states; } set { _states = value; } }

    static PlayerStates _instance;
    public static PlayerStates instance
    {
        get
        {
            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;

        _states = new PlayerStateFactory(this);
        _currentState = _states.Default();
        _currentState.EnterState(); 
    }

    void Update()
    {
        _currentState.UpdateState();
    }

    void FixedUpdate()
    {
        _currentState.FixedUpdateState();
    }
}
}