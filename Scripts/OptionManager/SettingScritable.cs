using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "PlayerSettings")]
public class SettingsScritable : ScriptableObject
{
    [Header("General")] 
    public float Gravity;
    [Header("MovementSetting")] 
    public int MovementSpeed;
    public int JumpForce;
    public float FlagCarrySpeed;
    [Header("WeaponAmmo")] 
    public int SheildAmmo;
    public int RocketLauncherAmmo;
    public int GrenadeLauncherAmmo;
    [Header("WeaponDamage")] 
    public float KnockBackWeaponDamage;
    public float MeleeWeaponDamage;
    public float RocketLauncherDamage;
    public float GrenadeLauncherDamage;
    [Header("WeaponCooldownTimer")] 
    public float KnockBackWeaponCooldown;
    public float MeleeWeaponCooldown;
    public float RocketLauncherCooldown;

    public float GranadeLauncherCooldown;

    
    public float GrenadeLauncherCooldown;
    public float ShieldCooldown;
    

    public void SaveSettings()
    {
        string path = Application.persistentDataPath + "/Settings.txt";
        //Write some text to the test.txt file

        StreamWriter writer = new StreamWriter(path, false);

        writer.Write($"Gravity\n{Gravity}\n");
        writer.Write($"MovementSpeed\n{MovementSpeed}\n");
        writer.Write($"JumpForce\n{JumpForce}\n");
        writer.Write($"FlagCarrySpeed\n{FlagCarrySpeed}\n");
        writer.Write($"ShieldAmmo\n{SheildAmmo}\n");
        writer.Write($"RocketLauncherAmmo\n{RocketLauncherAmmo}\n");
        writer.Write($"GrenadeLauncherAmmo\n{GrenadeLauncherAmmo}\n");
        writer.Write($"KnockBackWeaponDamage\n{KnockBackWeaponDamage}\n");
        writer.Write($"MeleeWeaponDamage\n{MeleeWeaponDamage}\n");
        writer.Write($"RocketLauncherDamage\n{RocketLauncherDamage}\n");
        writer.Write($"GrenadeLauncherDamage\n{GrenadeLauncherDamage}\n");
        writer.Write($"KnockBackWeaponCooldown\n{KnockBackWeaponCooldown}\n");
        writer.Write($"MeleeWeaponCooldown\n{MeleeWeaponCooldown}\n");
        writer.Write($"RocketLauncherCooldown\n{RocketLauncherCooldown}\n");
        writer.Write($"GrenadeLauncherCooldown\n{GrenadeLauncherCooldown}\n");

        writer.Close();

        StreamReader reader = new StreamReader(path);

        //Print the text from the file

        Debug.Log(reader.ReadToEnd());

        reader.Close();

        Debug.Log(Application.persistentDataPath + "/Settings.txt");
    }


    public void LoadSettings()
    {
        string path = Application.persistentDataPath + "/settings.txt";

        StreamReader reader = new StreamReader(path);


        var splitLine = reader.ReadToEnd().Split("\n");
        string newCase = "";
        foreach (var VARIABLE in splitLine)
        {
            bool isNumeric = float.TryParse(VARIABLE, out float n);

            if (!isNumeric)
            {
                newCase = VARIABLE;
            }
            else
            {
                switch (newCase)
                {
                    case "MovementSpeed":
                        MovementSpeed = (int)n;
                        break;
                    case "JumpForce":
                        JumpForce = (int)n;
                        break;
                    case "FlagCarrySpeed":
                        FlagCarrySpeed = n;
                        break;
                    case "ShieldAmmo":
                        SheildAmmo = (int)n;
                        break;
                    case "RocketLauncherAmmo":
                        RocketLauncherAmmo = (int)n;
                        break;
                    case "GrenadeLauncherAmmo":
                        GrenadeLauncherAmmo = (int)n;
                        break;
                    case "KnockBackWeaponDamage":
                        KnockBackWeaponDamage = n;
                        break;
                    case "MeleeWeaponDamage":
                        MeleeWeaponDamage = n;
                        break;
                    case "RocketLauncherDamage":
                        RocketLauncherDamage = n;
                        break;
                    case "GrenadeLauncherDamage":
                        GrenadeLauncherDamage = n;
                        break;
                    case "KnockBackWeaponCooldown":
                        KnockBackWeaponCooldown = n;
                        break;
                    case "MeleeWeaponCooldown":
                        MeleeWeaponCooldown = n;
                        break;
                    case "RocketLauncherCooldown":
                        RocketLauncherCooldown = n;
                        break;
                    case "GrenadeLauncherCooldown":
                        GrenadeLauncherCooldown = n;
                        break;
                    case "Gravity":
                        Gravity = n;
                        break;
                }
            }
        }

        

        reader.Close();
    }

    public void SetDefaultValues()
    {
      
        //Movement
        Gravity = 36f;
        MovementSpeed = 19;
        JumpForce = 17;
        FlagCarrySpeed = 0.83f;
        
        //Ammo
        RocketLauncherAmmo = 1;
        GrenadeLauncherAmmo = 5;
        SheildAmmo = 2;
        
        //Damage
        MeleeWeaponDamage = 0.2f;
        KnockBackWeaponDamage = 0.1f;
        //RocketLauncherDamage = ?? ToDo
        //GrenadeLauncherDamage = ?? ToDo
        
        //Cooldowns
        MeleeWeaponCooldown = 0.2f;
        KnockBackWeaponCooldown = 4;
        RocketLauncherCooldown = 30;
        GrenadeLauncherCooldown = 30;
        ShieldCooldown = 50;
    }
}