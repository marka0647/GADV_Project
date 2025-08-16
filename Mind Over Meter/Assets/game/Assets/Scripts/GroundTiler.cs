using UnityEngine;

public class GroundTiler : MonoBehaviour
{
    [SerializeField] private Transform target;        // Player to track along X
    [SerializeField] private GameObject tilePrefab;   // Your single ground sprite prefab (with collider)
    [SerializeField] private int visibleTiles = 3;    // 3+ tiles recommended
    [SerializeField] private float segmentLength = 0f;// 0 = auto-detect from prefab

    // NEW: stop tiling when camera stops following
    [Header("Stop with camera")]
    [SerializeField] private Camera followCamera;      // assign Main Camera (or auto-fills)
    [SerializeField] private float stopFollowAtX = Mathf.Infinity; // same X you use for camera stop
    [SerializeField] private bool lockTilingWhenCameraStops = true;
    private bool tilingLocked = false;

    private Transform[] tiles;

    void Start()
    {
        if (!target)
        {
            var p = FindFirstObjectByType<PlayerAlt>();
            if (p) target = p.transform;
        }
        if (!followCamera) followCamera = Camera.main;

        if (!tilePrefab)
        {
            Debug.LogError("GroundStripTiler: Tile Prefab is not assigned.");
            enabled = false;
            return;
        }

        // If not provided, auto-detect width from prefab bounds
        if (segmentLength <= 0f)
            segmentLength = MeasurePrefabWidth(tilePrefab);

        // Create tiles as children, laid out left->right
        tiles = new Transform[Mathf.Max(visibleTiles, 3)];
        float startX = target ? Mathf.Floor(target.position.x / segmentLength) * segmentLength : transform.position.x;

        for (int i = 0; i < tiles.Length; i++)
        {
            var t = Instantiate(tilePrefab, transform).transform;
            t.position = new Vector3(startX + i * segmentLength, transform.position.y, transform.position.z);
            tiles[i] = t;
        }

        // Ensure array is sorted left->right by X
        System.Array.Sort(tiles, (a, b) => a.position.x.CompareTo(b.position.x));
    }

    void Update()
    {
        if (tiles == null || tiles.Length == 0) return;

        // NEW: lock tiling once the camera hits the stop X
        if (!tilingLocked && lockTilingWhenCameraStops && followCamera != null)
        {
            // small tolerance to account for smoothing
            if (followCamera.transform.position.x >= stopFollowAtX - 0.01f)
                tilingLocked = true;
        }

        if (tilingLocked || !target) return; // stop recycling once camera stops following

        // Existing recycling driven by target X
        while (target.position.x - tiles[0].position.x > segmentLength)
        {
            Transform leftMost = tiles[0];
            Transform rightMost = tiles[tiles.Length - 1];

            float newX = rightMost.position.x + segmentLength;
            leftMost.position = new Vector3(newX, leftMost.position.y, leftMost.position.z);

            // Rotate array (left-most becomes new right-most)
            for (int i = 0; i < tiles.Length - 1; i++) tiles[i] = tiles[i + 1];
            tiles[tiles.Length - 1] = leftMost;
        }

        // Optional: if your player can run left, keep this block (or delete if he never runs left)
        while (tiles[tiles.Length - 1].position.x - target.position.x > segmentLength)
        {
            Transform rightMost = tiles[tiles.Length - 1];
            Transform leftMost = tiles[0];

            float newX = leftMost.position.x - segmentLength;
            rightMost.position = new Vector3(newX, rightMost.position.y, rightMost.position.z);

            // Rotate array (right-most becomes new left-most)
            for (int i = tiles.Length - 1; i > 0; i--) tiles[i] = tiles[i - 1];
            tiles[0] = rightMost;
        }
    }

    private float MeasurePrefabWidth(GameObject prefab)
    {
        // Instantiate a temporary tile to measure bounds in world units
        var tmp = Instantiate(prefab, new Vector3(100000f, 100000f, 100000f), Quaternion.identity);
        float width = 0f;

        var sr = tmp.GetComponent<SpriteRenderer>();
        if (sr) width = sr.bounds.size.x;

        if (width <= 0f)
        {
            var col2 = tmp.GetComponent<Collider2D>();
            if (col2) width = col2.bounds.size.x;
        }
        if (width <= 0f)
        {
            var col = tmp.GetComponent<Collider>();
            if (col) width = col.bounds.size.x;
        }
        if (width <= 0f)
        {
            var rend = tmp.GetComponent<Renderer>();
            if (rend) width = rend.bounds.size.x;
        }

        Destroy(tmp);
        if (width <= 0f) width = 10f; // fallback
        return width;
    }

}

