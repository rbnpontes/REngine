import { dotnet } from './_framework/dotnet.js'
import { init } from './librengine.js';

console.log('[REngine]: Initializing');
const [dotnetModule, libRengineExports] = await Promise.all([
    dotnet.create(),
    init(),
]);

const { setModuleImports, getAssemblyExports, getConfig } = dotnetModule;
setModuleImports('librengine.js', {...libRengineExports});

const exports = await getAssemblyExports(getConfig().mainAssemblyName);

console.log('[REngine]:', exports);
console.log('[REngine]: Running DotNet');
await dotnet.run();
console.log('[REngine]: Done');