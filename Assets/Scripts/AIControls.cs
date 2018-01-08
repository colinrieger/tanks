﻿using UnityEngine;
using UnityEngine.AI;

public class AIControls : MonoBehaviour
{
    public GameObject m_TargetTank;

    private Rigidbody m_TankRigidbody;
    private TankControls m_TankControls;
    private GameObject m_TankTurret;
    private GameObject m_TargetTankTurret;
    private NavMeshAgent m_NavMeshAgent;

    private const float c_MaxAngleToTargetToMove = 10f;
    private const float c_MaxAngleToTargetToFire = 1f;
    private const float c_DistanceToTargetOffset = 20f;
    private const float c_ShellRadius = 0.2f;

    private void Start()
    {
        m_TankRigidbody = GetComponent<Rigidbody>();
        m_TankControls = GetComponent<TankControls>();
        m_TankTurret = transform.Find("Renderers/Turret").gameObject;
        m_TargetTankTurret = m_TargetTank.transform.Find("Renderers/Turret").gameObject;

        m_NavMeshAgent = GetComponent<NavMeshAgent>();
        m_NavMeshAgent.speed = m_TankControls.m_Speed;
        m_NavMeshAgent.angularSpeed = m_TankControls.m_TankRotationSpeed;
        m_NavMeshAgent.autoBraking = false;
        m_NavMeshAgent.updatePosition = false;
        m_NavMeshAgent.updateRotation = false;
    }

    private void FixedUpdate()
    {
        if (m_TargetTank.activeSelf)
        {
            UpdateTank();
            UpdateTurret();
        }
    }

    private void UpdateTank()
    {
        m_NavMeshAgent.destination = m_TargetTank.transform.position;

        Vector3 targetDirection = m_NavMeshAgent.nextPosition - transform.position;
        Quaternion rotationToTarget = (targetDirection != Vector3.zero) ? Quaternion.LookRotation(targetDirection) : new Quaternion();
        rotationToTarget.x = 0;
        rotationToTarget.z = 0;

        Quaternion tankRotation = Quaternion.RotateTowards(transform.rotation, rotationToTarget, Time.deltaTime * m_TankControls.m_TankRotationSpeed);
        m_TankRigidbody.MoveRotation(tankRotation);

        float distanceToTarget = Vector3.Distance(transform.position, m_TargetTank.transform.position);
        float tankToTargetAngle = Quaternion.Angle(rotationToTarget, tankRotation);
        if (distanceToTarget > c_DistanceToTargetOffset && tankToTargetAngle < c_MaxAngleToTargetToMove)
        {
            Vector3 movement = m_TankRigidbody.transform.forward * m_TankControls.m_Speed * Time.deltaTime;
            m_TankRigidbody.MovePosition(m_TankRigidbody.position + movement);
        }
    }

    private void UpdateTurret()
    {
        Vector3 targetDirection = (m_TargetTankTurret.transform.position - m_TankTurret.transform.position);
        Quaternion rotationToTarget = Quaternion.LookRotation(targetDirection);
        rotationToTarget.x = 0;
        rotationToTarget.z = 0;

        Quaternion turretRotation = Quaternion.RotateTowards(m_TankTurret.transform.rotation, rotationToTarget, Time.deltaTime * m_TankControls.m_TurretRotationSpeed);
        m_TankTurret.transform.rotation = turretRotation;

        if (Quaternion.Angle(rotationToTarget, turretRotation) < c_MaxAngleToTargetToFire)
        {
            RaycastHit hit;
            if (Physics.SphereCast(m_TankTurret.transform.position, c_ShellRadius, targetDirection, out hit) && hit.transform == m_TargetTank.transform)
                m_TankControls.Fire();
        }
    }
}
