using UnityEngine;
using UnityEngine.UI;

public class FeedbackScrollView : MonoBehaviour
{
    [SerializeField] private Text feedbackTextPrefab;
    [SerializeField] private Transform contentParent;

    public void DisplayFeedbacks(System.Collections.Generic.List<RatingManager.PassengerFeedback> feedbacks)
    {
        // Limpiar feedbacks anteriores
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevos textos
        foreach (var feedback in feedbacks)
        {
            Text textItem = Instantiate(feedbackTextPrefab, contentParent);
            textItem.text = $"{feedback.passengerName}: {feedback.feedbackText}";
            textItem.color = feedback.isPositive ? Color.green : Color.red;
        }
    }
}