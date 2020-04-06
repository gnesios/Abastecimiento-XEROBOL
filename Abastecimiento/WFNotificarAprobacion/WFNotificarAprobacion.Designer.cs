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

namespace Abastecimiento.WFNotificarAprobacion
{
    public sealed partial class WFNotificarAprobacion
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
            System.Workflow.Runtime.CorrelationToken correlationtoken1 = new System.Workflow.Runtime.CorrelationToken();
            System.Workflow.ComponentModel.ActivityBind activitybind3 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind4 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind5 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind6 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind7 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind8 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.Activities.Rules.RuleConditionReference ruleconditionreference1 = new System.Workflow.Activities.Rules.RuleConditionReference();
            System.Workflow.ComponentModel.ActivityBind activitybind9 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind11 = new System.Workflow.ComponentModel.ActivityBind();
            System.Workflow.ComponentModel.ActivityBind activitybind10 = new System.Workflow.ComponentModel.ActivityBind();
            this.SolicitarAprobacion = new Microsoft.SharePoint.WorkflowActions.SendEmail();
            this.NotificarAprobacionAutomatica = new Microsoft.SharePoint.WorkflowActions.SendEmail();
            this.noAutonomia = new System.Workflow.Activities.IfElseBranchActivity();
            this.siAutonomia = new System.Workflow.Activities.IfElseBranchActivity();
            this.logNotificarAprobacion = new Microsoft.SharePoint.WorkflowActions.LogToHistoryListActivity();
            this.Autonomia = new System.Workflow.Activities.IfElseActivity();
            this.RecuperarInformacion = new System.Workflow.Activities.CodeActivity();
            this.onWorkflowActivated1 = new Microsoft.SharePoint.WorkflowActions.OnWorkflowActivated();
            // 
            // SolicitarAprobacion
            // 
            this.SolicitarAprobacion.BCC = null;
            activitybind1.Name = "WFNotificarAprobacion";
            activitybind1.Path = "cuerpoNotificacion";
            activitybind2.Name = "WFNotificarAprobacion";
            activitybind2.Path = "usuariosCopiados";
            correlationtoken1.Name = "workflowToken";
            correlationtoken1.OwnerActivityName = "WFNotificarAprobacion";
            this.SolicitarAprobacion.CorrelationToken = correlationtoken1;
            this.SolicitarAprobacion.From = null;
            this.SolicitarAprobacion.Headers = null;
            this.SolicitarAprobacion.IncludeStatus = false;
            this.SolicitarAprobacion.Name = "SolicitarAprobacion";
            activitybind3.Name = "WFNotificarAprobacion";
            activitybind3.Path = "asuntoNotificacion";
            activitybind4.Name = "WFNotificarAprobacion";
            activitybind4.Path = "usuariosNotificados";
            this.SolicitarAprobacion.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.BodyProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind1)));
            this.SolicitarAprobacion.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.CCProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind2)));
            this.SolicitarAprobacion.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.SubjectProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind3)));
            this.SolicitarAprobacion.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.ToProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind4)));
            // 
            // NotificarAprobacionAutomatica
            // 
            this.NotificarAprobacionAutomatica.BCC = null;
            activitybind5.Name = "WFNotificarAprobacion";
            activitybind5.Path = "cuerpoNotificacion";
            activitybind6.Name = "WFNotificarAprobacion";
            activitybind6.Path = "usuariosCopiados";
            this.NotificarAprobacionAutomatica.CorrelationToken = correlationtoken1;
            this.NotificarAprobacionAutomatica.From = null;
            this.NotificarAprobacionAutomatica.Headers = null;
            this.NotificarAprobacionAutomatica.IncludeStatus = false;
            this.NotificarAprobacionAutomatica.Name = "NotificarAprobacionAutomatica";
            activitybind7.Name = "WFNotificarAprobacion";
            activitybind7.Path = "asuntoNotificacion";
            activitybind8.Name = "WFNotificarAprobacion";
            activitybind8.Path = "usuariosNotificados";
            this.NotificarAprobacionAutomatica.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.CCProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind6)));
            this.NotificarAprobacionAutomatica.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.BodyProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind5)));
            this.NotificarAprobacionAutomatica.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.SubjectProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind7)));
            this.NotificarAprobacionAutomatica.SetBinding(Microsoft.SharePoint.WorkflowActions.SendEmail.ToProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind8)));
            // 
            // noAutonomia
            // 
            this.noAutonomia.Activities.Add(this.SolicitarAprobacion);
            this.noAutonomia.Name = "noAutonomia";
            // 
            // siAutonomia
            // 
            this.siAutonomia.Activities.Add(this.NotificarAprobacionAutomatica);
            ruleconditionreference1.ConditionName = "Tiene Autonomia";
            this.siAutonomia.Condition = ruleconditionreference1;
            this.siAutonomia.Name = "siAutonomia";
            // 
            // logNotificarAprobacion
            // 
            this.logNotificarAprobacion.Duration = System.TimeSpan.Parse("-10675199.02:48:05.4775808");
            this.logNotificarAprobacion.EventId = Microsoft.SharePoint.Workflow.SPWorkflowHistoryEventType.WorkflowComment;
            activitybind9.Name = "WFNotificarAprobacion";
            activitybind9.Path = "mensajeHistorial";
            this.logNotificarAprobacion.HistoryOutcome = "Finalizado";
            this.logNotificarAprobacion.Name = "logNotificarAprobacion";
            this.logNotificarAprobacion.OtherData = "";
            this.logNotificarAprobacion.UserId = -1;
            this.logNotificarAprobacion.SetBinding(Microsoft.SharePoint.WorkflowActions.LogToHistoryListActivity.HistoryDescriptionProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind9)));
            // 
            // Autonomia
            // 
            this.Autonomia.Activities.Add(this.siAutonomia);
            this.Autonomia.Activities.Add(this.noAutonomia);
            this.Autonomia.Name = "Autonomia";
            // 
            // RecuperarInformacion
            // 
            this.RecuperarInformacion.Name = "RecuperarInformacion";
            this.RecuperarInformacion.ExecuteCode += new System.EventHandler(this.RecuperarInformacion_ExecuteCode);
            activitybind11.Name = "WFNotificarAprobacion";
            activitybind11.Path = "workflowId";
            // 
            // onWorkflowActivated1
            // 
            this.onWorkflowActivated1.CorrelationToken = correlationtoken1;
            this.onWorkflowActivated1.EventName = "OnWorkflowActivated";
            this.onWorkflowActivated1.Name = "onWorkflowActivated1";
            activitybind10.Name = "WFNotificarAprobacion";
            activitybind10.Path = "workflowProperties";
            this.onWorkflowActivated1.SetBinding(Microsoft.SharePoint.WorkflowActions.OnWorkflowActivated.WorkflowIdProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind11)));
            this.onWorkflowActivated1.SetBinding(Microsoft.SharePoint.WorkflowActions.OnWorkflowActivated.WorkflowPropertiesProperty, ((System.Workflow.ComponentModel.ActivityBind)(activitybind10)));
            // 
            // WFNotificarAprobacion
            // 
            this.Activities.Add(this.onWorkflowActivated1);
            this.Activities.Add(this.RecuperarInformacion);
            this.Activities.Add(this.Autonomia);
            this.Activities.Add(this.logNotificarAprobacion);
            this.Name = "WFNotificarAprobacion";
            this.CanModifyActivities = false;

        }

        #endregion

        private Microsoft.SharePoint.WorkflowActions.SendEmail SolicitarAprobacion;

        private IfElseBranchActivity noAutonomia;

        private IfElseBranchActivity siAutonomia;

        private IfElseActivity Autonomia;

        private Microsoft.SharePoint.WorkflowActions.LogToHistoryListActivity logNotificarAprobacion;

        private Microsoft.SharePoint.WorkflowActions.SendEmail NotificarAprobacionAutomatica;

        private CodeActivity RecuperarInformacion;

        private Microsoft.SharePoint.WorkflowActions.OnWorkflowActivated onWorkflowActivated1;






























    }
}
