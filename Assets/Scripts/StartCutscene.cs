using UnityEngine;
using System.Collections;

public class StartCutscene : MonoBehaviour
{
    [Header("Hero Settings")]
    public HeroMove hero;
    public Transform heroTransform;
    public Animator heroAnimator;
    public Rigidbody2D heroRigidbody;

    [Header("Hero Movement")]
    public float heroStartX = -12f;
    public float heroEndX = -3f;
    public float groundY = -2f;                    // Уровень земли (подберите под вашу сцену)
    public string speedParam = "Speed";            // Параметр Speed в аниматоре
    public string groundedParam = "isGrounded";            // Параметр Speed в аниматоре

    [Header("Wall Settings")]
    public GameObject spikesWall;
    public float wallAppearDelay = 1.5f;
    public float wallMoveSpeed = 4f;
    public float wallStartX = -15f;
    public float wallEndX = -8f;

    [Header("Camera Settings")]
    public CameraMover cameraMover;
    public float cameraStartDelay = 0.5f;

    private enum CutsceneState
    {
        HeroMoving,
        WaitingForWall,
        WallMoving,
        Complete
    }

    private CutsceneState state = CutsceneState.HeroMoving;
    private float heroMoveDuration;
    private float heroMoveTimer = 0f;
    private float wallMoveTimer = 0f;
    private Vector3 heroStartPos;
    private Vector3 heroTargetPos;
    private Vector3 wallStartPos;
    private Vector3 wallTargetPos;
    private float heroEntrySpeed = 3f;

    void Start()
    {
        // Находим компоненты
        if (hero == null)
            hero = FindObjectOfType<HeroMove>();

        if (heroTransform == null && hero != null)
            heroTransform = hero.transform;

        if (heroAnimator == null && heroTransform != null)
            heroAnimator = heroTransform.GetComponent<Animator>();

        if (heroRigidbody == null && hero != null)
            heroRigidbody = hero.GetComponent<Rigidbody2D>();

        if (cameraMover == null)
            cameraMover = FindObjectOfType<CameraMover>();

        if (spikesWall == null)
            spikesWall = GameObject.FindGameObjectWithTag("SpikesWall");

        // Отключаем управление героем
        if (hero != null)
            hero.enabled = false;

        // Отключаем движение камеры
        if (cameraMover != null)
            cameraMover.enabled = false;

        // Отключаем физику героя
        if (heroRigidbody != null)
        {
            heroRigidbody.isKinematic = true;
            heroRigidbody.linearVelocity = Vector2.zero;
        }

        // Устанавливаем начальные позиции (на уровне земли)
        heroEntrySpeed = hero.moveSpeed;
        heroStartPos = new Vector3(heroStartX, groundY, heroTransform.position.z);
        heroTargetPos = new Vector3(heroEndX, groundY, heroTransform.position.z);
        heroTransform.position = heroStartPos;

        // Рассчитываем длительность движения героя
        float heroDistance = Mathf.Abs(heroEndX - heroStartX);
        heroMoveDuration = heroDistance / heroEntrySpeed;

        // Настраиваем стену
        if (spikesWall != null)
        {
            wallStartPos = new Vector3(wallStartX, spikesWall.transform.position.y, spikesWall.transform.position.z);
            wallTargetPos = new Vector3(wallEndX, spikesWall.transform.position.y, spikesWall.transform.position.z);
            spikesWall.transform.position = wallStartPos;
            spikesWall.SetActive(false);
        }

        Debug.Log($"Кат-сцена: начало - герой выходит с X={heroStartX}, Y={groundY}");
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        // Этап 1: Герой выходит на экран
        while (heroMoveTimer < heroMoveDuration)
        {
            heroMoveTimer += Time.deltaTime;
            float t = heroMoveTimer / heroMoveDuration;

            // Плавное движение героя
            heroTransform.position = Vector3.Lerp(heroStartPos, heroTargetPos, t);

            // Устанавливаем параметр Speed для анимации бега
            if (heroAnimator != null)
                heroAnimator.SetFloat(speedParam, heroEntrySpeed);
                heroAnimator.SetBool(groundedParam, true);

            yield return null;
        }

        // Фиксируем позицию героя
        heroTransform.position = heroTargetPos;

        // Сбрасываем параметр Speed (останавливаем анимацию бега)
        if (heroAnimator != null)
            heroAnimator.SetFloat(speedParam, 0f);
            heroAnimator.SetBool(groundedParam, false);

        // Включаем управление героем
        if (hero != null)
            hero.enabled = true;

        // Включаем физику героя
        if (heroRigidbody != null)
            heroRigidbody.isKinematic = false;

        Debug.Log("Кат-сцена: герой остановился, управление возвращено");

        // Этап 2: Ожидание перед появлением стены
        yield return new WaitForSeconds(wallAppearDelay);

        // Этап 3: Появление и движение стены
        if (spikesWall != null)
        {
            spikesWall.SetActive(true);
            Debug.Log("Кат-сцена: стена появилась");

            float wallMoveDuration = Mathf.Abs(wallEndX - wallStartX) / wallMoveSpeed;

            while (wallMoveTimer < wallMoveDuration)
            {
                wallMoveTimer += Time.deltaTime;
                float t = wallMoveTimer / wallMoveDuration;
                spikesWall.transform.position = Vector3.Lerp(wallStartPos, wallTargetPos, t);
                yield return null;
            }

            spikesWall.transform.position = wallTargetPos;
            Debug.Log("Кат-сцена: стена остановилась");
        }

        // Этап 4: Ожидание перед началом движения камеры
        yield return new WaitForSeconds(cameraStartDelay);

        // Этап 5: Запуск движения камеры
        if (cameraMover != null)
        {
            cameraMover.enabled = true;
            cameraMover.StartMoving();
            Debug.Log("Кат-сцена: камера начала движение");
        }

        Debug.Log("Кат-сцена: полностью завершена!");
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            // Рисуем путь героя
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(heroStartX, groundY, 0), new Vector3(0.5f, 1, 0));
            Gizmos.DrawWireCube(new Vector3(heroEndX, groundY, 0), new Vector3(0.5f, 1, 0));
            Gizmos.DrawLine(new Vector3(heroStartX, groundY, 0), new Vector3(heroEndX, groundY, 0));

            // Рисуем уровень земли
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(heroStartX - 2, groundY, 0), new Vector3(heroEndX + 2, groundY, 0));
        }
    }
}