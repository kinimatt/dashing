﻿namespace TopHat.Configuration {
  using System;
  using System.Data.Entity.Design.PluralizationServices;
  using System.Globalization;

  /// <summary>
  ///   The default convention.
  /// </summary>
  //// Dear ReSharper, these things were done on purpose so that people can extend off this
  //// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
  //// ReSharper disable once MemberCanBePrivate.Global
  public class DefaultConvention : IConvention {
    /// <summary>
    ///   The _string length.
    /// </summary>
    private readonly ushort stringLength;

    /// <summary>
    ///   The _decimal precision.
    /// </summary>
    private readonly byte decimalPrecision;

    /// <summary>
    ///   The _decimal scale.
    /// </summary>
    private readonly byte decimalScale;

    /// <summary>
    ///   The pluralizer.
    /// </summary>
    protected readonly PluralizationService Pluralizer;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DefaultConvention" /> class.
    /// </summary>
    /// <param name="stringLength">
    ///   The string length.
    /// </param>
    /// <param name="decimalPrecision">
    ///   The decimal precision.
    /// </param>
    /// <param name="decimalScale">
    ///   The decimal scale.
    /// </param>
    public DefaultConvention(ushort stringLength = 255, byte decimalPrecision = 18, byte decimalScale = 10) {
      this.stringLength = stringLength;
      this.decimalPrecision = decimalPrecision;
      this.decimalScale = decimalScale;
      this.Pluralizer = PluralizationService.CreateService(new CultureInfo("en-GB")); // <-- Americans, back in your box.
    }

    /// <summary>
    ///   The table for.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <returns>
    ///   The <see cref="string" />.
    /// </returns>
    public virtual string TableFor(Type entity) {
      return this.Pluralizer.Pluralize(entity.Name);
    }

    /// <summary>
    ///   The schema for.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <returns>
    ///   The <see cref="string" />.
    /// </returns>
    public virtual string SchemaFor(Type entity) {
      return null;
    }

    /// <summary>
    ///   The primary key of.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <returns>
    ///   The <see cref="string" />.
    /// </returns>
    public virtual string PrimaryKeyFor(Type entity) {
      return entity.Name + "Id";
    }

    /// <summary>
    ///   The is primary key auto generated.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <returns>
    ///   The <see cref="bool" />.
    /// </returns>
    public virtual bool IsPrimaryKeyAutoGenerated(Type entity) {
      return true;
    }

    /// <summary>
    ///   The string length for.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <param name="propertyName">
    ///   The property name.
    /// </param>
    /// <returns>
    ///   The <see cref="ushort" />.
    /// </returns>
    public ushort StringLengthFor(Type entity, string propertyName) {
      return this.stringLength;
    }

    /// <summary>
    ///   The decimal precision for.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <param name="propertyName">
    ///   The property name.
    /// </param>
    /// <returns>
    ///   The <see cref="byte" />.
    /// </returns>
    public byte DecimalPrecisionFor(Type entity, string propertyName) {
      return this.decimalPrecision;
    }

    /// <summary>
    ///   The decimal scale for.
    /// </summary>
    /// <param name="entity">
    ///   The entity.
    /// </param>
    /// <param name="propertyName">
    ///   The property name.
    /// </param>
    /// <returns>
    ///   The <see cref="byte" />.
    /// </returns>
    public byte DecimalScaleFor(Type entity, string propertyName) {
      return this.decimalScale;
    }
  }
}