using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationTest : MonoBehaviour
{
    [SerializeField] private DebugScriptableObject _debugSO;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Transform _anchorPoint;

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
    }

    // Update is called once per frame
    void Update()
    {
        //_rigidbody.transform.RotateAround(_anchorPoint.position, Vector3.up, -20 * Time.deltaTime);
    }

    public void RotateTestOne()
    {
        _debugSO.generalDebugMessage = "Rotate 1";
        //StartCoroutine(GoAroundCorner(_rigidbody.rotation));

        Quaternion initRotation = _rigidbody.rotation;
        Quaternion newRotation = initRotation * Quaternion.AngleAxis(-90, Vector3.right);
        Vector3 newPosition = _rigidbody.position - _rigidbody.transform.up;
        _rigidbody.MovePosition(newPosition);
        _rigidbody.MoveRotation(newRotation);
    }

    public void RotateTestTwo()
    {
        _debugSO.generalDebugMessage = "Rotate 2";

        Quaternion initRotation = _rigidbody.rotation;
        Quaternion newRotation = initRotation * Quaternion.AngleAxis(-45, Vector3.right);
        Vector3 newPosition = _rigidbody.position - _rigidbody.transform.up * 0.75f;
        _rigidbody.MovePosition(newPosition);
        _rigidbody.MoveRotation(newRotation);
    }

    public void RotateTestThree()
    {
        _debugSO.generalDebugMessage = "Rotate 3";
        
        _rigidbody.transform.Translate(-_rigidbody.transform.right / 2);

        


        StartCoroutine(ThirdTestCoroutine());
    }

    IEnumerator ThirdTestCoroutine()
    {
        Vector3 newPosition = _rigidbody.position - _rigidbody.transform.up / 2;
        //_rigidbody.MovePosition(newPosition);

        for (int i = 0; i < 9; i++)
        {
            _rigidbody.transform.RotateAround(_anchorPoint.position, Vector3.up, -10);
            yield return new WaitForFixedUpdate();
        }
    }

    public void RotateTestFour()
    {
        _debugSO.generalDebugMessage = "Rotate 4";
    }

    public void RotateTestFive()
    {
        _debugSO.generalDebugMessage = "Rotate 5";
    }

    IEnumerator GoAroundCorner(Quaternion pRotation)
    {
        Quaternion initRotation = pRotation;
        _rigidbody.velocity = Vector3.zero;
        
        Quaternion newRotation = initRotation * Quaternion.AngleAxis(90, Vector3.right);

        _rigidbody.rotation = Quaternion.Lerp(initRotation, newRotation, Time.deltaTime * 0.1f);

        Debug.Log("Drehung fertig");

        yield return null;
    }
}
