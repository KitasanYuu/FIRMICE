/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace cowsins {
/// <summary>
/// Super simple enemy script that allows any object with this component attached to receive damage,aim towards the player and shoot at it.
/// This is not definitive and it will 100% be modified and re structured in future updates
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [System.Serializable]
    public class Events
    {
        public UnityEvent OnSpawn, OnShoot, OnDamaged, OnDeath;
    }

    [Tooltip("Name of the enemy. This will appear on the killfeed"),SerializeField]
    protected string _name;

    protected float health; 
    [Tooltip("initial enemy health "), SerializeField] 
    protected float maxHealth;

    protected float shield; 
    [Tooltip("initial enemy shield"), SerializeField]
    protected float  maxShield;

    [Tooltip("display enemy status via UI"), SerializeField] 
    protected Slider healthSlider,shieldSlider;

    [Tooltip("If true, it will display the UI with the shield and health sliders, disabling this will disable pop ups."), SerializeField]
    public bool showUI;

    [Tooltip("Add a pop up showing the damage that has been dealt. Recommendation: use the already made pop up included in this package. "), SerializeField]
    private GameObject damagePopUp;

    [Tooltip("Colour for the specific status to be displayed in the slider"), SerializeField]
    private Color shieldColor, healthColor;

    [Tooltip("Horizontal randomness variation"), SerializeField]
    private float xVariation;

        [SerializeField]
        protected AudioClip dieSFX;

    [HideInInspector]public Transform player;

    public Events events;


    // Start is called before the first frame update"
    public virtual void Start()
    {
        // Status initial settings
        health = maxHealth;
        shield = maxShield;

        // Spawn
        events.OnSpawn.Invoke();

        // Initial settings 
        player = GameObject.FindGameObjectWithTag("Player").transform;


        // UI 
        // Determine max values
        if (healthSlider != null) healthSlider.maxValue = maxHealth;
        if ( shieldSlider != null) shieldSlider.maxValue = maxShield;
        if (!showUI) // Destroy the enemy UI if we do not want to display it
        {
            Destroy(healthSlider);
            Destroy(shieldSlider);
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    public virtual void Update()
    {
        //Handle UI 
        if (healthSlider != null) healthSlider.value = Mathf.Lerp(healthSlider.value, health,Time.deltaTime * 6);
        if (shieldSlider != null) shieldSlider.value = Mathf.Lerp(shieldSlider.value, shield, Time.deltaTime * 4); 

        // Manage health
        if (health <= 0) Die();
    }
    /// <summary>
    /// Since it is IDamageable, it can take damage, if a shot is landed, damage the enemy
    /// </summary>
    public virtual void Damage(float _damage)
    {
        float damage = Mathf.Abs(_damage); 
        float oldDmg = damage; 
        if (damage <= shield) // Shield will be damaged
        {
            shield -= damage;
            if (shieldSlider != null) shieldSlider.GetComponent<Animator>().Play("UIDamageShieldEnemy");
        }
        else
        {
            damage = damage - shield;
            shield = 0;
            health -= damage;
            if (shieldSlider != null) shieldSlider.GetComponent<Animator>().Play("UIDamageShieldEnemy");
            if (shieldSlider != null) healthSlider.GetComponent<Animator>().Play("UIDamageEnemy");
        }

        // Custom event on damaged
        events.OnDamaged.Invoke();
        UIEvents.onEnemyHit.Invoke();
        // If you do not want to show a damage pop up, stop, do not continue
        if (!showUI) return;
            GameObject popup = Instantiate(damagePopUp,transform.position,Quaternion.identity) as GameObject;
         if (oldDmg / Mathf.FloorToInt(oldDmg) == 1)
            popup.transform.GetChild(0).GetComponent<TMP_Text>().text = oldDmg.ToString("F0");
            else 
            popup.transform.GetChild(0).GetComponent<TMP_Text>().text = oldDmg.ToString("F1");
            float xRand = Random.Range(-xVariation, xVariation);
            popup.transform.position = popup.transform.position + new Vector3(xRand, 0, 0);
    }
    public virtual void Die()
    {
        // Custom event on damaged
        events.OnDeath.Invoke();

            SoundManager.Instance.PlaySound(dieSFX, 0, 1, false, 0);
        // Does it display killfeed on death? 
        UIEvents.onEnemyKilled.Invoke(_name);
        Destroy(this.gameObject);
    }
}
#if UNITY_EDITOR
[System.Serializable]
[CustomEditor(typeof(EnemyHealth))]
public class EnemyEditor : Editor
{

    override public void OnInspectorGUI()
    {
        serializedObject.Update();
        EnemyHealth myScript = target as EnemyHealth;

        EditorGUILayout.LabelField("IDENTITY", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_name"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("STATS", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxShield"));

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("showUI"));
        if (myScript.showUI)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthSlider"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldSlider"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("healthColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldColor"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("damagePopUp"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("xVariation"));
        }
        serializedObject.ApplyModifiedProperties();

    }
}
#endif
}