using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;

public class CarUpgrade : MonoBehaviour
{
    [SerializeField] private CarDamage _carDamage;
    [SerializeField] private WheelSpinner _wheelSpinner;

    [Header("Upgrade Stats")]
    [SerializeField] private float _upgradeEnginePerLevel;
    [SerializeField] private float _upgradeFuelTankPerLevel;
    [SerializeField] private float _upgradeCarBodyPerLevel;
    [SerializeField] private float _upgradePodveskaPerLevel;

    [Header("")]
    [SerializeField] private WheelJoint2D wheels;
    private JointSuspension2D upgradeSuspension;
    [Header("")]
     [Range(1, 5)] public int _engineLevel = 1;
     [Range(1, 5)] public int _fuelTankLevel = 1;
     [Range(1, 5)] public int _carBodyLevel = 1;
     [Range(1, 5)] public int _podveskaLevel = 1;

    void Start()
    {
        upgradeSuspension = wheels.suspension;
    }
    public void UpgradeEngine()
    {
        if (_engineLevel <= 5)
        {
            _engineLevel++;
            _wheelSpinner.maxSpeed += _upgradeEnginePerLevel * _engineLevel;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgrateFuelTank()
    {
        if (_fuelTankLevel <= 5)
        {
            _fuelTankLevel++;
            _wheelSpinner.maxFuel += _upgradeFuelTankPerLevel * _fuelTankLevel;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgradeCarBody()
    {
        if (_carBodyLevel <= 5)
        {
            _carBodyLevel++;
            _carDamage.maxHp += _upgradeCarBodyPerLevel * _carBodyLevel;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }

    public void UpgradePodveska()
    {
        if (_podveskaLevel <= 5)
        {
            _podveskaLevel++;
            upgradeSuspension.frequency += _upgradePodveskaPerLevel * _podveskaLevel;
            wheels.suspension = upgradeSuspension;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }
}

