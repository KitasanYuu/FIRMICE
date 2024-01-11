using UnityEngine;

namespace CustomInspector.Demo
{
    public class CI_ExampleScene : MonoBehaviour
    {
        [URL("http://mbservices.de/")]
        [Preview(Size.big)] public Sprite image;

        [HorizontalLine("Some examples")]


        [SerializeField] bool showVariables;

        [ShowIf(nameof(showVariables))]
        public bool var1;
        public bool var2;
        [ForceFill(errorMessage = "Please fill this")]
        public GameObject var3;
        [ForceFill("(0, 0)")]
        public Vector2 var4;

        [Tooltip("The Path where the generated mesh will be saved")]
#pragma warning disable IDE0090 // Use 'new(...)'
        public FolderPath somePath = new FolderPath();
#pragma warning restore IDE0090 // Use 'new(...)'

        [Mask(6)] public int mask1 = 17;

        [ReadOnly] public int id;

        [Space(20)]

        [HorizontalLine("buttons")]

        [HorizontalGroup]
        [Button(nameof(LogHelloWorld), tooltip = "Logs the most popular sentence (for computer scientists) in the console")]
        [HideField] public int _;

        [HorizontalGroup(size = 1.5f)]
        [Button(nameof(LogNumber), true, tooltip = "will log the entered number in the unity-console")]
        public int number = 7;

        void LogHelloWorld() { Debug.Log("Hello, World!"); }
        void LogNumber(int n) { Debug.Log($"Number: {n}"); }
    }

}