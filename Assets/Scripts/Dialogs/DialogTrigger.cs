using UnityEngine;
using UnityEngine.InputSystem;

namespace DefenderOfDreams.Dialogs
{
    public class DialogTrigger : MonoBehaviour
    {
        [SerializeField] private DialogAsset dialog;
        [SerializeField] private bool autoStart;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        private bool _playerInside;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _playerInside = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
                _playerInside = false;
        }

        private void Update()
        {
            if (!_playerInside || dialog == null)
                return;
            if (DialogRunner.Instance == null || DialogRunner.Instance.IsActive)
                return;

            if (autoStart)
            {
                DialogRunner.Instance.StartDialog(dialog);
                return;
            }

            var kb = Keyboard.current;
            if (kb == null)
                return;

            if (interactKey == KeyCode.E && kb.eKey.wasPressedThisFrame)
                DialogRunner.Instance.StartDialog(dialog);
            else if (interactKey == KeyCode.F && kb.fKey.wasPressedThisFrame)
                DialogRunner.Instance.StartDialog(dialog);
        }
    }
}
