
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        [SerializeField] private GameObject WinMenu;
        [SerializeField] private GameObject LosMenu;
        [SerializeField] private GameObject PlayerUI;
        [SerializeField] private Image fade;
        [SerializeField] private ParticleSystem trashParticle;

        private GameObject Trashes;
        private GameObject TrashBag;
        private GameObject correct;

        public UnityEvent events;

        public Outline[] Outlines;
        private void Start() {

            Outlines = FindObjectsOfType<Outline>();
            Trashes  = GameObject.Find("Trashes");
            TrashBag = GameObject.Find("Trash");
            correct  = GameObject.Find("Correct");
            allTrashCount = Trashes.transform.childCount;
            Trashes.SetActive(false);
            TrashBag.SetActive(false);
            correct.SetActive(false);

            allPropCount = FindObjectsOfType<Prop>().Length;
            instance = this;
            StartCoroutine(
            DelayEvent(4f,
            () => StartCoroutine(ImageFadeDark(fade, 1, () =>
            {
                Prop.Party = true;
                GameObject.Find("Characters").SetActive(false);
                Trashes.SetActive(true);
                StartCoroutine(ImageFadeClear(fade, 2f , () =>
                {
                    Prop.Party = false;
                    SetOutlines(true);
                    TrashBag.SetActive(true);
                    correct.SetActive(true);
                }));
            }))));

            SetOutlines(false);
        }
        private void SetOutlines(bool value)
        {
            for (int i = 0; i < Outlines.Length; i++)
            {
                Outlines[i].enabled = value;
            }
        }
        public void PauseGame() {
            // CcC
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F)) WinGame(); // hack 
            if (Input.GetKeyDown(KeyCode.KeypadEnter)) Prop.Party = false;
        }
        public static IEnumerator ImageFadeDark(Image image, float speed, Action callback)
        {
            while (image.color.a < 0.99f) {
                image.color += new Color(0,0,0, Time.deltaTime * speed);
                yield return new WaitForSecondsRealtime(0.02f);
            }
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);
            callback?.Invoke();
        }

        public static IEnumerator ImageFadeClear(Image image, float speed, Action callback)
        {
            while (image.color.a > 0.1f) {
                image.color -= new Color(0, 0, 0, Time.deltaTime * speed);
                yield return new WaitForSecondsRealtime(0.02f);
            }
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
            callback?.Invoke();
        }
        public static IEnumerator ImageOpenClose(Image image, float speed, Action callback)
        {
            yield return ImageFadeDark(image,speed, null);
            yield return ImageFadeClear(image, speed, null);
            callback?.Invoke();
        }

        public static IEnumerator DelayEvent(float delay, Action callback)
        {
            yield return new WaitForSecondsRealtime(delay);
            callback?.Invoke();
        }

        public void WinGame() {
            TrashBag.SetActive(false);
            StartCoroutine(DelayEvent(1f, () =>
            {
               if (PlayerUI) PlayerUI.SetActive(false);
               if (WinMenu)  WinMenu.SetActive(true);
               for (int i = 0; i < 10; i++) Debug.Log("win Game");
            }));
        }
        public void LosGame() {
            if (PlayerUI) PlayerUI.SetActive(false);
            if (WinMenu) LosMenu.SetActive(true);
            for (int i = 0; i < 10; i++) Debug.Log("los Game");
        }

        private int placedProps;
        private int allPropCount;
        private int allTrashCount;
        private int cleanedTrashCount;

        public void OnTrashAdded()
        {
            trashParticle.Play();
            cleanedTrashCount++;
            CheckWin();
        }

        private void CheckWin() {
            if (placedProps == allPropCount && cleanedTrashCount == allTrashCount)
            {
                WinGame();
            }
        }

        public void OnPropPlaced() {
            placedProps++;
            CheckWin();
        }

        public void RestartScene() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void NextScene() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
}