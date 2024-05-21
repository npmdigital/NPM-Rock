﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Enums.Group;

namespace Rock.Model
{
    /// <summary>
    /// Represents a system discovered relationship between two people.
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "PeerNetwork" )]
    [DataContract]
    public class PeerNetwork
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Key]
        [DataMember]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the source person alias identifier.
        /// </summary>
        /// <value>
        /// The source person alias identifier.
        /// </value>
        [DataMember]
        public int SourcePersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the target person alias identifier.
        /// </summary>
        /// <value>
        /// The target person alias identifier.
        /// </value>
        [DataMember]
        public int TargetPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the relationship type value identifier.
        /// </summary>
        /// <value>
        /// The relationship type value identifier.
        /// </value>
        [DataMember]
        public int RelationshipTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the relationship start date.
        /// </summary>
        /// <value>
        /// The relationship start date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime RelationshipStartDate { get; set; }

        /// <summary>
        /// Gets or sets the relationship end date.
        /// </summary>
        /// <value>
        /// The relationship end date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? RelationshipEndDate { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier.
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the classification entity identifier.
        /// </summary>
        /// <value>
        /// The classification entity identifier.
        /// </value>
        [DataMember]
        public int? ClassificationEntityId { get; set; }

        /// <summary>
        /// Gets or sets the relationship score.
        /// </summary>
        /// <value>
        /// The relationship score.
        /// </value>
        [DataMember]
        [DecimalPrecision( 8, 1 )]
        public decimal RelationshipScore { get; set; }

        /// <summary>
        /// Gets or sets the relationship score last update value.
        /// </summary>
        /// <value>
        /// The relationship score last update value.
        /// </value>
        [DataMember]
        [DecimalPrecision( 8, 1 )]
        public decimal RelationshipScoreLastUpdateValue { get; set; }

        /// <summary>
        /// Gets or sets the relationship trend.
        /// </summary>
        /// <value>
        /// The relationship trend.
        /// </value>
        [DataMember]
        public RelationshipTrend RelationshipTrend { get; set; }

        /// <summary>
        /// Gets or sets the last update date time.
        /// </summary>
        /// <value>
        /// The last update date time.
        /// </value>
        [DataMember]
        public DateTime LastUpdateDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the source person alias.
        /// </summary>
        /// <value>
        /// The source person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias SourcePersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the target person alias.
        /// </summary>
        /// <value>
        /// The target person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias TargetPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the relationship type value.
        /// </summary>
        /// <value>
        /// The relationship type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue RelationshipTypeValue { get; set; }

        #endregion
    }

    /// <summary>
    /// Peer Network Configuration class.
    /// </summary>
    public partial class PeerNetworkConfiguration : EntityTypeConfiguration<PeerNetwork>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PeerNetworkConfiguration"/> class.
        /// </summary>
        public PeerNetworkConfiguration()
        {
            this.HasKey( p => p.Id );
            this.HasRequired( a => a.SourcePersonAlias ).WithMany().HasForeignKey( p => p.SourcePersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.TargetPersonAlias ).WithMany().HasForeignKey( p => p.TargetPersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.RelationshipTypeValue ).WithMany().HasForeignKey( a => a.RelationshipTypeValueId ).WillCascadeOnDelete( false );
        }
    }
}