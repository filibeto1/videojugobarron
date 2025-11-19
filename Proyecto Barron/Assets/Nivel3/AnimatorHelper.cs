using UnityEngine;

public static class AnimatorHelper
{
    // Método de extensión para verificar parámetros del Animator
    public static bool HasParameter(Animator animator, int paramHash)
    {
        if (animator == null) return false;

        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.nameHash == paramHash)
                return true;
        }
        return false;
    }
}