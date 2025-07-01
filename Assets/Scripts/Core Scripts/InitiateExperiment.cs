using System;
using System.Resources;
using UnityEngine;

public class InitiateExperiment : MonoBehaviour
{
   [Header("Debug Options")]
   public bool exp;

   
   [Header("Experiment Start Settings")]
   
   [SerializeField] private GameObject laserDummy;
   [SerializeField] private GameObject waterBlockDummy;
   [SerializeField] GameObject glassBlockDummy;

   [SerializeField] private GameObject laserRotationPivot;
   [SerializeField] private GameObject leftSideMaterilDrop;
   [SerializeField] private GameObject glassDropLocation;
   [SerializeField] private GameObject canvasVisualControlls;
   [SerializeField] private GameObject tableCanvas;
   [SerializeField] private GameObject dummyTables;
   [SerializeField] private GameObject expetimentText;
   [SerializeField] private GameObject experimentPointLight; 
   


   private void Start()
   {
      if (exp)
      {
         expetimentText.SetActive(false);
         laserDummy.SetActive(false);
         waterBlockDummy.SetActive(false);
         glassBlockDummy.SetActive(false);
         laserRotationPivot.SetActive(true);
         leftSideMaterilDrop.SetActive(true);
         glassDropLocation.SetActive(true);
         canvasVisualControlls.SetActive(true);
         tableCanvas.SetActive(true);
         dummyTables.SetActive(true);
         experimentPointLight.SetActive(true);
      }
      else
      {
         expetimentText.SetActive(true);
         laserDummy.SetActive(true);
         waterBlockDummy.SetActive(true);
         glassBlockDummy.SetActive(true);
         laserRotationPivot.SetActive(false);
         leftSideMaterilDrop.SetActive(false);
         glassDropLocation.SetActive(false);
         canvasVisualControlls.SetActive(false);
         tableCanvas.SetActive(false);
         dummyTables.SetActive(false);
         experimentPointLight.SetActive(false);
      }
      
   }

   public void InitizeExperiment()
   {
      Debug.Log("Test");
      expetimentText.SetActive(false);
      laserDummy.SetActive(false);
      waterBlockDummy.SetActive(false);
      glassBlockDummy.SetActive(false);
      laserRotationPivot.SetActive(true);
      leftSideMaterilDrop.SetActive(true);
      glassDropLocation.SetActive(true);
      canvasVisualControlls.SetActive(true);
      tableCanvas.SetActive(true);
      dummyTables.SetActive(true);
      experimentPointLight.SetActive(true);
   }
}
