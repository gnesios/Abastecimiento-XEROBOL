using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;

namespace AbastecimientoCustomActions.Layouts.AbastecimientoCustomActions
{
    /// <summary>
    /// Deshabilita temporalmente la ejecucion de Eventos de lista
    /// </summary>
    class DisabledItemEventsScope : SPItemEventReceiver, IDisposable
    {
        public DisabledItemEventsScope()
        {
            base.EventFiringEnabled = false;
            //base.DisableEventFiring();
        }

        #region IDisposable Members
        public void Dispose()
        {
            base.EventFiringEnabled = true;
            //base.EnableEventFiring();
        }
        #endregion
    }
}
