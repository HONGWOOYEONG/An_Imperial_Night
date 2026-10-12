using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class FieldExitTrigger : MonoBehaviour
{
    [SerializeField] private string meleeDealer = "MeleeDealer";
    [SerializeField] private string rangedTag = "RangedDealer";
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject Boss;
    [SerializeField] private GameObject BossHealthUI;

    [SerializeField] private ParallaxManager parallaxManager;

    [Header("Boss Intro")]
    [SerializeField] private float bossIntroDuration = 5f;
    [SerializeField] private float bossZoomSize = 5f;
    [SerializeField] private AnimationClip bossIntroClip;


    private bool closeRangeInside = false;
    private bool rangedInside = false;
    private bool isNextMap = false;
    private bool bossSequenceStarted = false;


    void Awake()
    {
        Boss.SetActive(false);
        BossHealthUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(meleeDealer))
        {
            closeRangeInside = true;
        }
        else if (other.CompareTag(rangedTag))
        {
            rangedInside = true;
        }

        TryProceed();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(meleeDealer))
        {
            closeRangeInside = false;
        }
        else if (other.CompareTag(rangedTag))
        {
            rangedInside = false;
        }

        if(isNextMap && !bossSequenceStarted)
        {
            bossSequenceStarted = true;
            BossHealthUI.SetActive(false);
            StartCoroutine(TakeWall());
            StartCoroutine(BossIntroSequence());
        }
    }

    private void TryProceed()
    {
        if (closeRangeInside && rangedInside)
        {
            isNextMap = true;
            parallaxManager.SetRoom(1);
            wall.SetActive(false);
            GameSessionManager.Instance.OnFieldExitPointReached();
        }
    }

    IEnumerator TakeWall()
    {
        yield return new WaitForSeconds(3f);
        wall.SetActive(true);
    }

    private IEnumerator BossIntroSequence()
    {
        GameObject introActor = null;
        PlayableGraph introGraph = default;

        yield return new WaitForSeconds(1f);

        // Combat activation must not depend on camera or animation playback succeeding.
        StartCoroutine(ActivateRealBossAfterIntro());

        try
        {
            introActor = CreateBossIntroActor(out introGraph);
            Transform cameraTarget = introActor != null ? introActor.transform : Boss.transform;

            if (CameraManager.Instance != null)
                yield return CameraManager.Instance.PlayBossIntro(
                    cameraTarget,
                    bossIntroDuration,
                    bossZoomSize);
            else
                yield return new WaitForSeconds(bossIntroDuration);
        }
        finally
        {
            if (introGraph.IsValid())
                introGraph.Destroy();
            if (introActor != null)
                Destroy(introActor);
        }
    }

    private IEnumerator ActivateRealBossAfterIntro()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, bossIntroDuration));
        Boss.SetActive(true);
        BossHealthUI.SetActive(true);
    }

    private GameObject CreateBossIntroActor(out PlayableGraph graph)
    {
        graph = default;

        SpriteRenderer sourceRenderer = Boss.GetComponentInChildren<SpriteRenderer>(true);
        if (sourceRenderer == null || bossIntroClip == null)
            return null;

        GameObject actor = new GameObject("BossIntroActor");
        actor.transform.SetPositionAndRotation(
            Boss.transform.position,
            Boss.transform.rotation);
        actor.transform.localScale = Boss.transform.lossyScale;

        SpriteRenderer renderer = actor.AddComponent<SpriteRenderer>();
        renderer.sprite = sourceRenderer.sprite;
        renderer.sharedMaterial = sourceRenderer.sharedMaterial;
        renderer.color = sourceRenderer.color;
        renderer.flipX = sourceRenderer.flipX;
        renderer.flipY = sourceRenderer.flipY;
        renderer.sortingLayerID = sourceRenderer.sortingLayerID;
        renderer.sortingOrder = sourceRenderer.sortingOrder;

        Animator animator = actor.AddComponent<Animator>();
        animator.applyRootMotion = false;
        actor.AddComponent<BossIntroEventSink>();

        graph = PlayableGraph.Create("BossIntroGraph");
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "BossIntro", animator);
        AnimationClipPlayable playable = AnimationClipPlayable.Create(graph, bossIntroClip);
        output.SetSourcePlayable(playable);
        graph.Play();

        return actor;
    }
}

// Receives combat animation events while the visual-only intro actor is playing.
public sealed class BossIntroEventSink : MonoBehaviour
{
    public void AE_Pattern_End() { }
}

