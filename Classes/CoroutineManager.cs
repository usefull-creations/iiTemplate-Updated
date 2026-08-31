using UnityEngine;

namespace StupidTemplate.Classes;

public class CoroutineManager : MonoBehaviour
{
    public static CoroutineManager Instance;

    private void Awake() => Instance = this;
}