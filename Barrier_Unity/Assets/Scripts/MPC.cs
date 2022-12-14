using UnityEngine;

public class MPC : MonoBehaviour
{
   [SerializeField] private GameObject _fastener;
   [SerializeField] private LineRenderer _cableRenderer;
   [SerializeField] private GameObject _beam;
   [SerializeField] private Buoy _buoy;
   private TorpedoDetectionModel _torpedoDetectionModel;
   private bool _deactivate = true;
   

   public float BeamLengthCoef { get; set; } = 0;
   private float _distToShip;
   private float _offsetFromWater = 0.25f; //can't see under water

   private void drawFastener()
   {
      if (!gameObject.activeSelf)
         return;

      _cableRenderer.startWidth = 0.1f;
      _cableRenderer.endWidth = 0.1f;
      _cableRenderer.SetPositions(new[] { gameObject.transform.position, _fastener.transform.position });
      _cableRenderer.useWorldSpace = true;
   }

   private void Start()
   {
      _torpedoDetectionModel = FindObjectOfType<TorpedoDetectionModel>();
   }

   void Update()
   {
      drawBeam();

      if (_deactivate && Scenario.Instance.TargetDetectStatus == TargetDetectStatus.Buoys)
      {
         _buoy.Deactivate();
         _torpedoDetectionModel.ClearRegression();
         _deactivate = false;
      }
   }

   private void drawBeam()
   {
      if (!gameObject.activeSelf)
         return;

      if (Scenario.Instance.TargetInfo == null ||
          Scenario.Instance.TargetDetectStatus == TargetDetectStatus.NoDetect ||
          Scenario.Instance.TargetDetectStatus == TargetDetectStatus.Buoys ||
          Scenario.Instance.TargetDetectStatus == TargetDetectStatus.MPCAndBuoys)
      {
         _beam.SetActive(false);
         return;
      }

      _beam.SetActive(true);
      
      Vector3 targetPoint = Scenario.Instance.TargetDetectStatus == TargetDetectStatus.MPCOnly
         ? Scenario.Instance.PointOfFirstDetectionByMPC 
         : Scenario.Instance.TargetInfo.Target.transform.position;


      var beamDir = targetPoint - gameObject.transform.position;
      var beamRender = _beam.GetComponent<LineRenderer>();
      beamRender.startWidth = 0.5f;
      beamRender.endWidth = 100f;
      beamRender.SetPositions(new[] { gameObject.transform.position, gameObject.transform.position + beamDir*BeamLengthCoef } );
      beamRender.useWorldSpace = true;
   }

   public void SetUpSettings(bool isActive, float distToShip)
   {
      gameObject.SetActive(isActive);
      _distToShip = distToShip;
      LabelHelper.ShowLabel(gameObject);

      int bc = VarSync.GetInt(VarName.NumBuoys);
      if(bc == 0) // one buoy
         _buoy.Born();
   }

   public void SetPosition(Vector3 newShipPos, Vector3 shipForward)
   {
      if (!gameObject.activeSelf)
         return;

      newShipPos.y += _offsetFromWater; 
      gameObject.transform.position = newShipPos - shipForward * _distToShip;
      drawFastener();
   }
}
