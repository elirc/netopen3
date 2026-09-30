"""Extract the current selection method into a new external disposable lab."""
import argparse
import hashlib
import json
from pathlib import Path


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output', required=True)
    parser.add_argument('--mode', choices=['reference', 'starter'], default='reference')
    args = parser.parse_args()
    here = Path(__file__).resolve().parent
    workspace = here.parents[3]
    output = Path(args.output).expanduser()
    if not output.is_absolute():
        parser.error('Output must be an absolute path.')
    output = output.resolve()
    if output.exists() or output == workspace or workspace in output.parents:
        parser.error('Output must be a new directory outside the source workspace.')
    source = workspace / 'Umbraco-CMS/src/Umbraco.Cms.Api.Management/Controllers/ManagementApiControllerBase.cs'
    raw = source.read_bytes()
    text = raw.decode('utf-8-sig')
    anchor = '    protected static List<TEntity> OrderByRequestedIds<TEntity>'
    if text.count(anchor) != 1:
        raise ValueError('Expected exactly one current helper signature.')
    start = text.index(anchor)
    brace = text.index('{', start)
    depth = 1
    end = brace + 1
    # The inspected method has no braces in string literals or comments.
    # Refuse changed structure rather than extracting an ambiguous method.
    while depth and end < len(text):
        depth += (text[end] == '{') - (text[end] == '}')
        end += 1
    if depth:
        raise ValueError('Unbalanced helper braces.')
    method = text[start:end]
    required = ['where TEntity : IEntity', 'entitiesById.TryAdd(entity.Key, entity);',
                'entitiesById.Remove(requestedId, out TEntity? entity)', 'return ordered;']
    if not all(method.count(item) == 1 for item in required) or '"' in method:
        raise ValueError('Helper changed; review extraction before generating a lab.')
    copied = method
    mutation = []
    if args.mode == 'starter':
        copied = method.replace('entitiesById.Remove(requestedId, out TEntity? entity)',
                                'entitiesById.TryGetValue(requestedId, out TEntity? entity)')
        mutation = ['Remove changed to TryGetValue: repeated requested IDs can emit repeatedly.']
    program = (here / 'Program.cs.txt').read_text(encoding='utf-8')
    selection = ('using System;\nusing System.Collections.Generic;\n'
                 'public interface IEntity { Guid Key { get; } }\n'
                 'public class Selection {\n' + copied + '\n}\n'
                 'public sealed class Harness : Selection {\n'
                 ' public static List<T> Select<T>(IEnumerable<T> entities, Guid[] keys) where T : IEntity\n'
                 ' => OrderByRequestedIds(entities, keys);\n}\n')
    output.mkdir(parents=True, exist_ok=False)
    (output / 'Selection.cs').write_text(selection, encoding='utf-8')
    (output / 'Program.cs').write_text(program, encoding='utf-8')
    (output / 'Lab.csproj').write_text('<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework><ImplicitUsings>enable</ImplicitUsings><Nullable>enable</Nullable></PropertyGroup></Project>\n', encoding='utf-8')
    (output / 'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>\n', encoding='utf-8')
    (output / 'SOURCE-MAP.json').write_text(json.dumps({
        'source': str(source), 'source_sha256': hashlib.sha256(raw).hexdigest(),
        'extracted_method_sha256': hashlib.sha256(method.encode('utf-8')).hexdigest(),
        'copied_method_sha256': hashlib.sha256(copied.encode('utf-8')).hexdigest(),
        'mode': args.mode, 'deliberate_mutations': mutation,
        'limitation': 'Exact method body with minimal IEntity Key contract; not the full controller or HTTP host.'
    }, indent=2) + '\n', encoding='utf-8')
    print(str(output))


if __name__ == '__main__':
    main()
