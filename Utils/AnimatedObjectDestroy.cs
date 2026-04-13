using UnityEngine;

public class AnimatedObjectDestroy : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Update is called once per frame
    void Start()
    {
        Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
    }
}
