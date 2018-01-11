﻿using UnityEngine;

public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankLayerMask;
    public ParticleSystem m_ExplosionParticles;
    public float m_MaxDamage = 50f;
    public float m_Force = 1000f;
    public float m_Radius = 5f;
    public float m_Lifetime = 10f;

    private Rigidbody m_ShellRigidbody;

    private void Start()
    {
        m_ShellRigidbody = GetComponent<Rigidbody>();

        Destroy(gameObject, m_Lifetime);
    }

    private void FixedUpdate()
    {
        transform.rotation = Quaternion.LookRotation(m_ShellRigidbody.velocity);
    }

    private void OnTriggerEnter(Collider other)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_Radius, m_TankLayerMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
            if (!targetRigidbody)
                continue;

            targetRigidbody.AddExplosionForce(m_Force, transform.position, m_Radius);

            TankControls tankControls = targetRigidbody.GetComponent<TankControls>();
            if (tankControls != null)
                tankControls.ApplyDamage(CalculateDamage(targetRigidbody.position));
        }
        
        m_ExplosionParticles.transform.parent = null;
        m_ExplosionParticles.Play();

        ParticleSystem.MainModule mainModule = m_ExplosionParticles.main;
        Destroy(m_ExplosionParticles.gameObject, mainModule.duration);

        Destroy(gameObject);
    }

    private float CalculateDamage(Vector3 targetPosition)
    {
        Vector3 explosionToTarget = targetPosition - transform.position;
        float relativeDistance = (m_Radius - explosionToTarget.magnitude) / m_Radius;

        return Mathf.Max(0f, relativeDistance * m_MaxDamage);
    }
}