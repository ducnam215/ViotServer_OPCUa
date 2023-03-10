using Opc.Ua.Server;
using Opc.Ua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using BatchPlant;

namespace ViotServer_OPCUa
{
    internal class BatchPlantNodeManager : CustomNodeManager2
    {

        public BatchPlantNodeManager(IServerInternal server, ApplicationConfiguration configuration)
        :
       base(server, configuration)
        {
            SystemContext.NodeIdFactory = this;

            // set one namespace for the type model and one names for dynamically created nodes.
            string[] namespaceUrls = new string[2];
            namespaceUrls[0] = BatchPlant.Namespaces.BatchPlant;
            namespaceUrls[1] = BatchPlant.Namespaces.BatchPlant + "/Instance";
            SetNamespaces(namespaceUrls);

            // get the configuration for the node manager.
            m_configuration = configuration.ParseExtension<BatchPlantServerConfiguration>();

            // use suitable defaults if no configuration exists.
            if (m_configuration == null)
            {
                m_configuration = new BatchPlantServerConfiguration();
            }
        }


        protected override NodeStateCollection LoadPredefinedNodes(ISystemContext context)
        {
            NodeStateCollection predefinedNodes = new NodeStateCollection();
            predefinedNodes.LoadFromBinaryResource(context,
                "C:/Users/Pham Duc Nam/Desktop/OPC_UA_Server/ViotServer_OPCUa/ViotServer_OPCUa/BatchPlant.PredefinedNodes.uanodes",
                typeof(BatchPlantNodeManager).GetTypeInfo().Assembly,
                true);

            return predefinedNodes;
        }

        public override void CreateAddressSpace(IDictionary<NodeId, IList<IReference>> externalReferences)
        {
            lock (Lock)
            {
                LoadPredefinedNodes(SystemContext, externalReferences);

                // find the untyped Batch Plant 1 node that was created when the model was loaded.
                BaseObjectState passiveNode = (BaseObjectState)FindPredefinedNode(new NodeId(BatchPlant.Objects.BatchPlant1, NamespaceIndexes[0]), typeof(BaseObjectState));

                // convert the untyped node to a typed node that can be manipulated within the server.
                m_batchPlant1 = new BatchPlantState(null);
                m_batchPlant1.Create(SystemContext, passiveNode);

                // replaces the untyped predefined nodes with their strongly typed versions.
                AddPredefinedNode(SystemContext, m_batchPlant1);

                m_batchPlant1.StartProcess.OnCallMethod = new GenericMethodCalledEventHandler(OnStartProcess);
                m_batchPlant1.StopProcess.OnCallMethod = new GenericMethodCalledEventHandler(OnStopProcess);

                m_simulationTimer = new System.Threading.Timer(DoSimulation, null, 1000, 1000);

            }
        }

        public void DoSimulation(object state)
        {
            var rand = new Random();

            int minValue = 20, maxValue = 30;
            double sensorValue;

            sensorValue = rand.NextDouble() * (maxValue - minValue) + minValue;
            //txtSensorValue.Text = sensorValue.ToString("#.##");
            m_batchPlant1.Mixer.LoadcellTransmitter.Output.Value = sensorValue;
        }

        private ServiceResult OnStartProcess(ISystemContext context, MethodState method, IList<object> inputArguments,
    IList<object> outputArguments)
        {

            return ServiceResult.Good;
        }

        private ServiceResult OnStopProcess(ISystemContext context, MethodState method, IList<object> inputArguments,
IList<object> outputArguments)
        {

            return ServiceResult.Good;
        }

        private BatchPlantServerConfiguration m_configuration;
        private static BatchPlantState m_batchPlant1;
        private System.Threading.Timer m_simulationTimer;


    }
}
