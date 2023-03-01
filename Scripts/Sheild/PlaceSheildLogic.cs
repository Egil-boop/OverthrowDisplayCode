 private void PlaceShield()
    {
        Vector3 headPos = gameObject.transform.position;
        Vector3 lookDir = gameObject.transform.forward;


        if (Physics.Raycast(headPos, lookDir, out RaycastHit hit, 10f))
        {
            Vector3 hitPos = hit.point;
            Vector3 up = hit.normal;
            Vector3 right = Vector3.Cross(up, lookDir).normalized;
            Vector3 forward = Vector3.Cross(right, up);

            float floorCheck = Vector3.Dot(Vector3.up, up);
            
            
            shieldHollowInstance.GetComponent<MeshRenderer>().material =
                floorCheck < 0.6 || weapons[currentWeaponIndex].OnCooldown() ? notAllowedMaterial : allowedMaterial;
            
            
            Quaternion rotation
                = Quaternion.LookRotation(forward, up);

            Matrix4x4 shieldToWorld = Matrix4x4.TRS(hitPos, rotation, Vector3.one);

            Vector3 point = new Vector3(0, 1, 0);
            Vector3 worldPoint = shieldToWorld.MultiplyPoint3x4(point);

            shieldHollowInstance.SetActive(true);
            shieldHollowInstance.transform.position = worldPoint;
            shieldHollowInstance.transform.rotation = rotation;
        }
        else
        {
            shieldHollowInstance.SetActive(false);
        }
    }