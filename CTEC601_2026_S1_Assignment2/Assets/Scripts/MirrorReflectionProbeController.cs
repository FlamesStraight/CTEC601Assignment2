using UnityEngine;

/// <summary>
/// Controller to keep a reflection probe for a mirror on the other side of the mirror plane
/// </summary>
///
[RequireComponent(typeof(ReflectionProbe))]
[ExecuteInEditMode]
public class MirrorReflectionProbeController : MonoBehaviour
{
	[Tooltip("The XZ plane to use as mirror")]
	public Transform Mirror;
	

	public void Start()
	{
		m_ReflectionProbe = GetComponent<ReflectionProbe>();
	}

	public void Update()
	{
		if (Mirror == null) return;

		Plane p = new Plane(Mirror.up, Mirror.position);
		Vector3 pointOnMirror = p.ClosestPointOnPlane(Camera.main.transform.position);
		Vector3 delta = Camera.main.transform.position - pointOnMirror;
		m_ReflectionProbe.transform.position = Camera.main.transform.position - 2 * delta;
		m_ReflectionProbe.RenderProbe();
	}

	private ReflectionProbe m_ReflectionProbe;
}
