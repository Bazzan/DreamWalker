using System;
using System.Collections;
using GP2_Team7.Objects.Cameras;
using UnityEngine;
using UnityEngine.UI;

namespace GP2_Team7.Managers
{
    public class UIManager : MonoBehaviour
    {
        private Transform _transform;

        private BlackBarHandler _barHandler;

        private Animator _fadeBlackScreen;

        private Coroutine _fadeRoutine;

        private static System.Action<bool> _onHasFaded = delegate { };

        /// <summary>
        /// (Un)Subscribe to the event that occurs when the screen
        /// has faded in/out.
        /// </summary>
        /// <param name="action">The action to subscribe/unsubscribe. Argument tells whether the recently finished process was a fade-out operation or not.</param>
        /// <param name="isSubscription">If true, the action gets subscribed, else unsubscribed.</param>
        public static void SubscribeOnHasFaded(System.Action<bool> action, bool isSubscription)
        {
            if (isSubscription)
                _onHasFaded += action;
            else
                _onHasFaded -= action;
        }

        private void Awake()
        {
            _transform = transform;

            _barHandler = Camera.main.GetComponent<BlackBarHandler>();

            _fadeBlackScreen = _transform.Find("FadeBlackScreen").GetComponent<Animator>();
        }

        public static void FadeScreen(bool toBlack, float timeScale = 1, float postFadeDelay = 0)
        {
            GameManager.GameUIManager.FadeScreenInst(toBlack, timeScale, postFadeDelay);
        }

        private void FadeScreenInst(bool toBlack, float timeScale, float postFadeDelay)
        {
            foreach (Image image in _fadeBlackScreen.GetComponentsInChildren<Image>())
            {
                image.color = Color.black;
            }

            string animName = toBlack ? "FadeOut" : "FadeIn";

            _fadeBlackScreen.speed = timeScale;

            if (!_fadeBlackScreen.GetCurrentAnimatorStateInfo(0).IsName(animName))
                _fadeBlackScreen.Play(animName, 0, 0);
            else
                return;

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
            }

            _fadeRoutine = StartCoroutine(FadeBlackEventDelay(animName, postFadeDelay));
        }

        public static void FadeScreen(bool toBlack, Color screenColor, float timeScale = 1, float postFadeDelay = 0)
        {
            foreach (Image image in GameManager.GameUIManager._fadeBlackScreen.GetComponentsInChildren<Image>())
            {
                image.color = screenColor;
            }

            GameManager.GameUIManager.FadeScreenInst(toBlack, timeScale, postFadeDelay);
        }

        public static void DisplayLoadingText(bool display)
        {
            GameObject loading = GameManager.GameUIManager._transform.Find("Loading").gameObject;

            loading.SetActive(display);
        }

        private IEnumerator FadeBlackEventDelay(string animName, float postFadeDelay)
        {
            const string fadeOut = "FadeOut";

            yield return new WaitUntil(() => !_fadeBlackScreen.GetCurrentAnimatorStateInfo(0).IsName(animName));

            if (postFadeDelay > 0)
                yield return new WaitForSeconds(postFadeDelay);

            _onHasFaded(animName.CompareTo(fadeOut) == 0);
        }
    }
}
