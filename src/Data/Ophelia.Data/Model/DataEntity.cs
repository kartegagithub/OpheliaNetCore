﻿using Newtonsoft.Json;
using Ophelia.Data.Attributes;
using Ophelia.Data.Model.Proxy;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ophelia.Data.Model
{
    [Serializable]
    public abstract class DataEntity : IDisposable, ITrackedEntity
    {
        private DataEntityTracker _Tracker;

        [System.Xml.Serialization.XmlIgnore]
        [JsonIgnore]
        public PocoEntityTracker Tracker
        {
            get
            {
                if (this._Tracker == null)
                    this._Tracker = new DataEntityTracker(this);
                return this._Tracker;
            }
            set
            {

            }
        }

        internal DataEntityTracker InternalTracker => (DataEntityTracker)this.Tracker;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long ID { get { return this.GetValue(op => op.ID); } set { this.SetValue(op => op.ID, value); } }

        [DataProperty(255)]
        public string Name { get { return this.GetValue(op => op.Name); } set { this.SetValue(op => op.Name, value); } }

        [DataProperty(255)]
        public string Code { get { return this.GetValue(op => op.Code); } set { this.SetValue(op => op.Code, value); } }
        public DateTime DateCreated { get { return this.GetValue(op => op.DateCreated); } set { this.SetValue(op => op.DateCreated, value); } }
        public DateTime DateModified { get { return this.GetValue(op => op.DateModified); } set { this.SetValue(op => op.DateModified, value); } }
        public long UserCreatedID { get { return this.GetValue(op => op.UserCreatedID); } set { this.SetValue(op => op.UserCreatedID, value); } }
        public long UserModifiedID { get { return this.GetValue(op => op.UserModifiedID); } set { this.SetValue(op => op.UserModifiedID, value); } }
        public long StatusID { get { return this.GetValue(op => op.StatusID); } set { this.SetValue(op => op.StatusID, value); } }

        private void SetTracker(DataEntityTracker tracker)
        {
            this._Tracker = tracker;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
        }
    }
}
