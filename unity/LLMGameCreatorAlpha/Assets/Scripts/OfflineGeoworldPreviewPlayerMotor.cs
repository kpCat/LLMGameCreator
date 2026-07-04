using UnityEngine;

namespace LLMGameCreatorAlpha
{
    public sealed class OfflineGeoworldPreviewPlayerMotor : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = 4f;
        [SerializeField] private Vector2 syntheticPosition;
        [SerializeField] private string currentSyntheticChunkKey = string.Empty;

        public Vector2 SyntheticPosition { get { return syntheticPosition; } }
        public string CurrentSyntheticChunkKey { get { return currentSyntheticChunkKey; } }

        private void Update()
        {
            var delta = Vector2.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                delta.y += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                delta.y -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                delta.x += 1f;
            }

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                delta.x -= 1f;
            }

            if (delta.sqrMagnitude <= 0f)
            {
                return;
            }

            ApplyManualMovement(delta.normalized * movementSpeed * Time.deltaTime);
        }

        public void ApplyManualMovement(Vector2 delta)
        {
            syntheticPosition += delta;
            transform.localPosition = new Vector3(syntheticPosition.x, 0.25f, syntheticPosition.y);
        }

        public void SnapToSample(int sampleIndex, string syntheticChunkKey)
        {
            syntheticPosition = new Vector2(sampleIndex * 1.5f, sampleIndex % 2);
            currentSyntheticChunkKey = syntheticChunkKey;
            transform.localPosition = new Vector3(syntheticPosition.x, 0.25f, syntheticPosition.y);
        }
    }
}
