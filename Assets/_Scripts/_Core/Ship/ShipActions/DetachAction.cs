using StarWriter.Core;

public class DetachAction : ShipActionAbstractBase
{
    ShipStatus shipData;

    void Start()
    {
        shipData = ship.GetComponent<ShipStatus>();
    }
    public override void StartAction()
    {
        if (shipData.Attached)
        {
            shipData.Attached = false;
            shipData.AttachedTrailBlock = null;
        }
    }

    public override void StopAction()
    {
    }
}