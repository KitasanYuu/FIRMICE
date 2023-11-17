using UnityEngine;
using cowsins; 
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace cowsins {
/// <summary>
/// Another example of Enemy.cs inheritance.
/// </summary>
public class CircularTargetEnemy : EnemyHealth
{
    [Tooltip("Direction of the movement. Whenever it reaches the end, it will go the opposite direction"),SerializeField] private Vector3 movementDirection;

    [Tooltip("Amount of time that the target will keep moving on the same direction"),SerializeField] private float directionDuration; 

    [Tooltip("Target velocity magnitude"),SerializeField] private float speed;

    [SerializeField] private float timeToRevive;

    private Vector3 movementDir;

    private float movementTimer; 
    
    private bool isDead = false;


    public override void Start()
    {
        // Since we override the start method, make sure to call the base function
        base.Start();

        // Set the default movement variables
        movementDir = movementDirection;
        movementTimer = directionDuration; 
    }

    public override void Update()
    {
        // Since we override the update method, make sure to call the base function
        base.Update();

        // If the enemy is dead, stop calling the movement 
        if (isDead) return; 

        // Apply the position
        transform.position = Vector3.MoveTowards(transform.position, transform.position + transform.TransformDirection(movementDir), speed * Time.deltaTime);

        // Timer related code.
        // Once the timer is equal or less than 0, the movement should be reset and change directions
        movementTimer -= Time.deltaTime; 
        if (movementTimer <= 0) ResetMovement(); 
    }

    // Change directions and reset timer
    private void ResetMovement()
    {
        movementDir = -movementDir;
        movementTimer = directionDuration;
    }
    // Simple damage function override
    public override void Damage(float damage)
    {
        if (isDead) return; 
        base.Damage(damage);
    }
    // Simple Die function override
    public override void Die()
    {
        if (isDead) return; // If the target is already dead, dont call this again
        // Set to dead
        isDead = true;
        // Call custom event
        events.OnDeath.Invoke();

        // Disable any possible hit effects
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            if (child != null)
                child.SetActive(false);
        }
        
            SoundManager.Instance.PlaySound(dieSFX, 0, 0, false, 0);
        // Invoke revive method
        Invoke("Revive", timeToRevive);

    }
    private void Revive()
    {
        // Revive
        isDead = false;
        // Reset variables related to health
        health = maxHealth;
        shield = maxShield;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            if (child != null)
                child.SetActive(true);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CircularTargetEnemy))]
public class CircularTargetEnemyEditor : Editor
{
    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        var myScript = target as CircularTargetEnemy;

        EditorGUILayout.LabelField("CIRCULAR TARGET", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("This Component uses a custom inspector to display relevant variables regarding the Circular Target Enemy. It is worth to mention " +
            "that this script inherits from Enemy.cs, so it is a damageable object.", EditorStyles.helpBox);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("movementDirection"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("directionDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("speed"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToRevive"));

        if (myScript.showUI) myScript.showUI = false; 
        serializedObject.ApplyModifiedProperties();

    }
}
#endif
}