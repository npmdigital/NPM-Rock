using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using LoxSmoke.DocXml;

using Rock;

namespace BlockGenerator.FileGenerators
{
    public class TypeScriptViewModelGenerator : Generator
    {
        private readonly DocXmlReader _viewModelXmlDoc;

        public TypeScriptViewModelGenerator()
        {
            var assemblyFileName = new FileInfo( new Uri( typeof( Rock.ViewModel.Utility.IViewModel ).Assembly.CodeBase ).LocalPath ).FullName;

            try
            {
                _viewModelXmlDoc = new DocXmlReader( assemblyFileName.Substring( 0, assemblyFileName.Length - 4 ) + ".xml" );
            }
            catch
            {
                _viewModelXmlDoc = null;
            }
        }

        public string GenerateViewModelForType( Type type )
        {
            var typeComment = _viewModelXmlDoc?.GetTypeComments( type )?.Summary?.StripHtml();

            return GenerateViewModel( GetClassNameForType( type ), typeComment, type.GetProperties().ToList() );
        }

        public string GenerateViewModel( string typeName, string typeComment, IList<PropertyInfo> properties )
        {
            var imports = new List<TypeScriptImport>();

            var sb = new StringBuilder();

            AppendCommentBlock( sb, typeComment, 0 );
            sb.AppendLine( $"export type {typeName} = {{" );

            var sortedProperties = properties.OrderBy( p => p.Name ).ToList();

            for ( int i = 0; i < sortedProperties.Count; i++ )
            {
                var property = properties[i];
                var isNullable = !IsNonNullType( property.PropertyType );

                if ( i > 0 )
                {
                    sb.AppendLine();
                }

                AppendCommentBlock( sb, property, 4 );

                sb.Append( $"    {property.Name.CamelCase()}" );

                if ( isNullable )
                {
                    sb.Append( "?" );
                }

                var (tsName, propertyImports) = GetTypeScriptType( property.PropertyType, !isNullable );
                sb.AppendLine( $": {tsName};" );

                imports.AddRange( propertyImports );
            }

            sb.AppendLine( "};" );

            // Remove recursive references to self.
            imports = imports.Where( i => i.DefaultImport != typeName && i.NamedImport != typeName ).ToList();

            return GenerateTypeScriptFile( imports, sb.ToString() );
        }

        private void AppendCommentBlock( StringBuilder sb, MemberInfo memberInfo, int indentationSize )
        {
            var xdoc = _viewModelXmlDoc?.GetMemberComment( memberInfo )?.StripHtml();

            AppendCommentBlock( sb, xdoc, indentationSize );
        }

        private void AppendCommentBlock( StringBuilder sb, string comment, int indentationSize )
        {
            if ( comment.IsNullOrWhiteSpace() )
            {
                return;
            }

            if ( comment.Contains( "\r\n" ) )
            {
                comment = comment.Replace( "\r\n", $"\r\n{new string( ' ', indentationSize )} * " );

                sb.AppendLine( $"{new string( ' ', indentationSize )}/**" );
                sb.AppendLine( $"{new string( ' ', indentationSize )} * {comment}" );
                sb.AppendLine( $"{new string( ' ', indentationSize )} */" );
            }
            else
            {
                sb.AppendLine( $"{new string( ' ', indentationSize )}/** {comment} */" );
            }
        }

        private static bool IsNonNullType( Type type )
        {
            return type.IsPrimitive;
        }

        /// <summary>
        /// Gets the TypeScript definition type of the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A string that contains the definition, such as "boolean | null".</returns>
        private static (string type, IList<TypeScriptImport> imports) GetTypeScriptType( Type type, bool isRequired )
        {
            var imports = new List<TypeScriptImport>();
            var underlyingType = Nullable.GetUnderlyingType( type );
            var isNullable = underlyingType != null;

            if ( isNullable )
            {
                type = underlyingType;
            }

            // Default to "unknown" type
            var tsType = "unknown";

            var isNumeric = type == typeof( byte )
                || type == typeof( sbyte )
                || type == typeof( short )
                || type == typeof( ushort )
                || type == typeof( int )
                || type == typeof( uint )
                || type == typeof( long )
                || type == typeof( ulong )
                || type == typeof( decimal )
                || type == typeof( float )
                || type == typeof( double );

            if ( type == typeof( bool ) )
            {
                tsType = "boolean";
            }
            else if ( isNumeric )
            {
                tsType = "number";
            }
            else if ( type == typeof( string ) )
            {
                tsType = "string";
                isNullable = isNullable || !isRequired;
            }
            else if ( type == typeof( DateTime ) || type == typeof( DateTimeOffset ) )
            {
                tsType = "string";
                isNullable = isNullable || !isRequired;
            }
            else if ( type == typeof( Guid ) )
            {
                tsType = "Guid";
                imports.Add( new TypeScriptImport
                {
                    SourcePath = "@Obsidian/Types",
                    NamedImport = "Guid"
                } );
                isNullable = isNullable || !isRequired;
            }
            else if ( type.IsGenericParameter )
            {
                tsType = type.Name;
                isNullable = isNullable || !isRequired;
            }
            else if ( type.IsArray )
            {
                var (itemType, itemImports) = GetTypeScriptType( type.GetElementType(), true );

                tsType = $"{itemType}[]";
                imports.AddRange( itemImports );
                isNullable = isNullable || !isRequired;
            }
            else if ( type.IsGenericType )
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();

                if ( genericTypeDefinition == typeof( Dictionary<,> ) )
                {
                    var (keyType, keyImports) = GetTypeScriptType( type.GetGenericArguments()[0], true );
                    var (valueType, valueImports) = GetTypeScriptType( type.GetGenericArguments()[1], true );

                    tsType = $"Record<{keyType}, {valueType}>";
                    imports.AddRange( keyImports );
                    imports.AddRange( valueImports );
                    isNullable = isNullable || !isRequired;
                }
                else if ( typeof( ICollection<> ).MakeGenericType( type.GenericTypeArguments[0] ).IsAssignableFrom( type ) )
                {
                    var (itemType, itemImports) = GetTypeScriptType( type.GenericTypeArguments[0], true );

                    tsType = $"{itemType}[]";
                    imports.AddRange( itemImports );
                    isNullable = isNullable || !isRequired;
                }
            }
            else if ( type.Namespace.StartsWith( "Rock.ViewModel" ) && ( type.Name.EndsWith( "Bag" ) || type.Name.EndsWith( "Box" ) ) )
            {
                var path = $"{type.Namespace.Substring( 14 ).Trim( '.' ).Replace( '.', '/' )}/{type.Name.CamelCase()}";
                tsType = type.Name;
                imports.Add( new TypeScriptImport
                {
                    SourcePath = $"@Obsidian/ViewModel/{path}",
                    NamedImport = type.Name
                } );
                isNullable = isNullable || !isRequired;
            }

            if ( tsType == "unknown" )
            {
            }

            if ( isNullable )
            {
                return ($"{tsType} | null", imports);
            }

            return (tsType, imports);
        }

        private static string GetClassNameForType( Type type )
        {
            if ( type.IsGenericType )
            {
                var genericTypes = type.GetGenericArguments().Select( t => t.Name ).ToList();

                return $"{type.Name.Split( '`' )[0]}<{string.Join( ", ", genericTypes )}>";
            }

            return type.Name;
        }
    }
}
