using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewPassengerFeedback", menuName = "Yurei Ressha/Passenger Feedback")]
public class PassengerFeedbackSO : ScriptableObject
{
    public string passengerName;
    public Sprite passengerSprite;
    
    [TextArea(2, 3)]
    public string feedbackPositivoEjemplo = "¡Excelente servicio!";
    
    [TextArea(2, 3)]
    public string feedbackNegativoEjemplo = "No me gustó el viaje...";
    
    public float baseRatingImpact = 0.2f;
}