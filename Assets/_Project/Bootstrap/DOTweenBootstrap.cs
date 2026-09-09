namespace Game.Bootstrap
{
    using DG.Tweening;
    using UnityEngine;

    public static class DOTweenBootstrap
    {
        private const int TweenCapacity = 500;
        private const int SequenceCapacity = 100;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            DOTween.SetTweensCapacity(TweenCapacity, SequenceCapacity);
        }
    }
}
