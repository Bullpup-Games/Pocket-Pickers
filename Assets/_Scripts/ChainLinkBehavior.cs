using UnityEngine;

public class ChainLinkBehavior : MonoBehaviour
{
    public int timesHit = 0;
    public HingeJoint2D joint; // Joint on this link
    public HingeJoint2D jointToLower; // Joint on the link below 
    private Collider2D _col;

    void Awake() {
        if (!joint) joint = GetComponent<HingeJoint2D>();
        _col = GetComponent<Collider2D>();
    }

    public void BreakFromAbove() {
        if (joint) Destroy(joint);
    }

    public void BreakFromBelow()
    {
        if (jointToLower) Destroy(jointToLower);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Card")) return;

        // Break on the third hit
        // TODO: Change sprite to progressivly more broken pieces
        timesHit++;
        if (timesHit != 3) return;

        var contactPoint = col.GetContact(0).point;
        var centerY = _col.bounds.center.y;
        var hitTopHalf = contactPoint.y > centerY;

        if (hitTopHalf) BreakFromAbove();
        else BreakFromBelow();
    }
}
