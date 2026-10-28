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
     [Range(1, 5)] public int _engineLevel = 0;
     [Range(1, 5)] public int _fuelTankLevel = 0;
     [Range(1, 5)] public int _carBodyLevel = 0;
     [Range(1, 5)] public int _podveskaLevel = 0;

    void Start()
    {
        upgradeSuspension = wheels.suspension;
    }
    public void UpgradeEngine()
    {
        if (_engineLevel <= 5)
        {
            _wheelSpinner.maxSpeed += _upgradeEnginePerLevel * _engineLevel;
            _engineLevel++;
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
            _wheelSpinner.maxFuel += _upgradeFuelTankPerLevel * _fuelTankLevel;
            _fuelTankLevel++;
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
            _carDamage.maxHp += _upgradeCarBodyPerLevel * _carBodyLevel;
            _carBodyLevel++;
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
            upgradeSuspension.frequency += _upgradePodveskaPerLevel * _podveskaLevel;
            wheels.suspension = upgradeSuspension;
            _podveskaLevel++;
        }
        else
        {
            Debug.Log("Max Level");
        }
    }
}

