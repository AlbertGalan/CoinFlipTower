using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIMenuGravity : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Gravity")]
    public float gravity = 2000f;
    public bool gravityInverted = false;

    [Header("Motion")]
    public float maxSpeed = 2000f;
    public float bounce = 0.2f;
    public float damping = 0.98f;
    public bool useUnscaledTime = true;

    [Header("Bounds")]
    public RectTransform boundsRect;
    public Vector2 padding = new Vector2(20f, 20f);

    [Header("Activation")]
    public bool startInactive = true;
    public float clickImpulse = 0f;

    [Header("Visual Indicator")]
    public Image indicatorTarget;
    public Color indicatorColor = Color.cyan;
    public float indicatorScale = 1.05f;
    public Sprite indicatorSprite;

    private RectTransform rectTransform;
    private Vector2 velocity;
    private Vector2 initialAnchoredPosition;
    private bool isActive;
    private bool rightClickHeld;
    private Vector3 initialScale;
    private Color initialIndicatorColor = Color.white;
    private Sprite initialIndicatorSprite;
    private Button buttonComponent;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialAnchoredPosition = rectTransform.anchoredPosition;
        isActive = !startInactive;
        initialScale = rectTransform.localScale;
        buttonComponent = GetComponent<Button>();
        if (indicatorTarget == null)
        {
            indicatorTarget = GetComponent<Image>();
        }
        if (indicatorTarget != null)
        {
            initialIndicatorColor = indicatorTarget.color;
            initialIndicatorSprite = indicatorTarget.sprite;
        }
        if (boundsRect == null && transform.parent != null)
        {
            boundsRect = transform.parent as RectTransform;
        }
    }

    private void Update()
    {
        if (!isActive)
        {
            rectTransform.anchoredPosition = initialAnchoredPosition;
            return;
        }

        if (boundsRect == null)
        {
            return;
        }

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }

        float gravityDir = gravityInverted ? 1f : -1f;
        velocity.y += gravity * gravityDir * dt;
        velocity.y = Mathf.Clamp(velocity.y, -maxSpeed, maxSpeed);
        velocity *= damping;

        Vector2 pos = rectTransform.anchoredPosition;
        pos += velocity * dt;

        Rect bounds = boundsRect.rect;
        float halfWidth = rectTransform.rect.width * 0.5f;
        float halfHeight = rectTransform.rect.height * 0.5f;

        float minX = bounds.xMin + padding.x + halfWidth;
        float maxX = bounds.xMax - padding.x - halfWidth;
        float minY = bounds.yMin + padding.y + halfHeight;
        float maxY = bounds.yMax - padding.y - halfHeight;

        if (pos.x < minX)
        {
            pos.x = minX;
            velocity.x *= -bounce;
        }
        else if (pos.x > maxX)
        {
            pos.x = maxX;
            velocity.x *= -bounce;
        }

        if (pos.y < minY)
        {
            pos.y = minY;
            velocity.y *= -bounce;
        }
        else if (pos.y > maxY)
        {
            pos.y = maxY;
            velocity.y *= -bounce;
        }

        rectTransform.anchoredPosition = pos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightClickHeld = true;
            SetIndicator(true);
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Activate();
            if (rightClickHeld)
            {
                return;
            }
            if (clickImpulse != 0f)
            {
                AddImpulse(new Vector2(0f, clickImpulse));
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            rightClickHeld = false;
            SetIndicator(false);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        if (rightClickHeld)
        {
            ToggleGravity();
            eventData.Use();
        }
    }

    public void Activate()
    {
        isActive = true;
    }

    public void ResetToInitial()
    {
        isActive = false;
        velocity = Vector2.zero;
        rectTransform.anchoredPosition = initialAnchoredPosition;
        SetIndicator(false);
    }

    public void ToggleGravity()
    {
        gravityInverted = !gravityInverted;
    }

    public void SetGravityInverted(bool inverted)
    {
        gravityInverted = inverted;
    }

    public void AddImpulse(Vector2 impulse)
    {
        velocity += impulse;
    }

    private void SetIndicator(bool active)
    {
        if (indicatorTarget != null)
        {
            indicatorTarget.color = active ? indicatorColor : initialIndicatorColor;
            if (indicatorSprite != null)
            {
                indicatorTarget.sprite = active ? indicatorSprite : initialIndicatorSprite;
            }
        }
        rectTransform.localScale = active ? initialScale * indicatorScale : initialScale;
        
        // Bloquear el botón en modo gravedad
        if (buttonComponent != null)
        {
            buttonComponent.interactable = !active;
        }
    }
}
