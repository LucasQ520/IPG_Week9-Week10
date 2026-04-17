using UnityEngine;

public class NPCVisitor : MonoBehaviour
{
    public float moveSpeed = 2f;

    private Vector3 targetPosition;
    private bool moving;

    public VisitorData visitorData;

    public void Setup(VisitorData data, Vector3 stopPoint)
    {
        visitorData = data;
        targetPosition = stopPoint;
        moving = true;

        ApplyVisualTraits();
    }

    void Update()
    {
        if (!moving)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        Vector3 direction = targetPosition - transform.position;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
        {
            moving = false;
        }
    }

    public void WalkTo(Vector3 destination)
    {
        targetPosition = destination;
        moving = true;
    }

    void ApplyVisualTraits()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend == null) return;

        if (visitorData.isPseudoman)
        {
            if (!visitorData.eyeColorMatches)
            {
                rend.material.color = Color.red;
            }
            else
            {
                rend.material.color = new Color(0.7f, 0.9f, 1f);
            }
        }
        else
        {
            rend.material.color = Color.white;
        }
    }

    public bool HasReachedTarget()
    {
        return !moving;
    }
}