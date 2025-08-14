using Unity.Hierarchy;
using UnityEngine;

public class Opponent : MonoBehaviour
{
    public GameObject GameManager;
    private GameManager m_GameManager;

    private float m_distance = 0;
    private float m_velo = 10;

    private void Start()
    {
       m_GameManager = GameManager.GetComponent<GameManager>();
        if (m_GameManager == null)
        {
            Debug.Log("AAAA");
        }
    }

    private void Update()
    {
        m_distance += m_velo * Time.deltaTime;
        float distanceInBetween = m_distance - m_GameManager.GetPlayerDistance();

        transform.position = new Vector3(-6 + distanceInBetween, -2.7f, 0);
    }
}
