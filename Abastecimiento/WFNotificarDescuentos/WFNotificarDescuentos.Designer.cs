using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Reflection;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;

namespace Abastecimiento.WFNotificarDescuentos
{
    public sealed partial class WFNotificarDescuentos
    {
        #region Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        private void InitializeComponent()
        {
            this.CanModifyActivities = true;
            System.Workflow.ComponentModel.ActivityBind activitybind1 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind2 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind3 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.Runtime.CorrelationToken correlationtoken1 = new System.Workflow.Runtime.CorrelationToken();
            System.Workflow.ComponentModel.ActivityBind activitybind4 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind5 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind7 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind6 = new System.Workflow.ComponentModel.ActivityBind();
            this.logNotificarDescuentos = new Microsoft.SharePoint.WorkflowActions.LogToHistoryListActivity();
            this.notificarDescuentos = new Microsoft.SharePoint.WorkflowActions.SendEmail();
            this.RecuperarValoresItem = new System.Workflow.Activities.CodeActivity();
            this.onWorkflowActivated1 = new Microsoft.SharePoint.WorkflowActions.OnWorkflowActivated();
            // 
            // logNotificarDescuentos
            // 
            this.logNotificarDescuentos.Duration = System.TimeSpan.Parse("-10675199.02:48:05.4775808");
            this.logNotificarDescuentos.EventId = Microsoft.SharePoint.Workflow.SPWorkflowHistoryEventType.WorkflowComment;
            activitybind1.Name = "WFNotificarDescuentos";
            activitybind1.Path = "mensajeHistorial";
            this.logNotificarDescuentos.HistoryOutcome = "Finalizado";
            this.logNotificarDescuentos.Name = "logNotificarDescuentos";
            this.logNotificarDescuentos.OtherData = "";
            this.logNotificarDescuentos.UserId = -1;
            this.logNotificarDescuentos.SetBinding(Microsoft.SharePoint.WorkflowActions.LogToHistoryListActivity.HistoryDescriptionProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind1)));
            // 
            // notificarDescuentos
            // 
            this.notificarDescuentos.BCC = null;
            activitybind2.Name = "WFNotificarDescuentos";
            activitybind2.Path = "cuerpoNotificacion";
            activitybind3.Name = "WFNotificarDescuentos";
            activitybind3.Path = "usuariosNotificadosCC";
            correlationtoken1.Name = "workflowToken";
            correlationtoken1.OwnerActivityName = "WFNotificarDescuentos";
            this.notificarDescuentos.CorrelationToken = correlationtoken1;
            this.notificarDescuentos.From = null;
            this.notificarDescuentos.Headers = null;
            this.notificarDescuentos.IncludeStatus = false;
            this.notificarDescuentos.Name = "notificarDescuentos";
            activitybind4.Name = "WFNotificarDescuentos";
            activitybind4.Path = "asuntoNotificacion";
            activitybind5.Name = "WFNotificarDescuentos";
            activitybind5.Path = "usuariosNotificados";
            this.notificarDescuentos.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.BodyProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind2)));
            this.notificarDescuentos.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.CCProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind3)));
            this.notificarDescuentos.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.ToProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind5)));
            this.notificarDescuentos.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.SubjectProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind4)));
            // 
            // RecuperarValoresItem
            // 
            this.RecuperarValoresItem.Name = "RecuperarValoresItem";
            this.RecuperarValoresItem.ExecuteCode += new System.EventHandler(this.RecuperarValoresItem_ExecuteCode);
            activitybind7.Name = "WFNotificarDescuentos";
            activitybind7.Path = "workflowId";
            // 
            // onWorkflowActivated1
            // 
            this.onWorkflowActivated1.CorrelationToken = correlationtoken1;
            this.onWorkflowActivated1.EventName = "OnWorkflowActivated";
            this.onWorkflowActivated1.Name = "onWorkflowActivated1";
            activitybind6.Name = "WFNotificarDescuentos";
            activitybind6.Path = "workflowProperties";
            this.onWorkflowActivated1.SetBinding(Microsoft.SharePoint.WorkflowActions.OnWorkflowActivated.WorkflowIdProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind7)));
            this.onWorkflowActivated1.SetBinding(Microsoft.SharePoint.WorkflowActions.OnWorkflowActivated.WorkflowPropertiesProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind6)));
            // 
            // WFNotificarDescuentos
            // 
            this.Activities.Add(this.onWorkflowActivated1);
            this.Activities.Add(this.RecuperarValoresItem);
            this.Activities.Add(this.notificarDescuentos);
            this.Activities.Add(this.logNotificarDescuentos);
            this.Name = "WFNotificarDescuentos";
            this.CanModifyActivities = false;

        }

        #endregion

        private CodeActivity RecuperarValoresItem;

        private Microsoft.SharePoint.WorkflowActions.SendEmail notificarDescuentos;

        private Microsoft.SharePoint.WorkflowActions.LogToHistoryListActivity logNotificarDescuentos;

        private Microsoft.SharePoint.WorkflowActions.OnWorkflowActivated onWorkflowActivated1;





































    }
}
