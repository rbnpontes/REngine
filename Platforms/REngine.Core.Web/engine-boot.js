import { dotnet } from './_framework/dotnet.js';
import { init as initDriver } from './librengine.js';
import { init as initCore } from './libenginecore.js';

/**
 * Initialize REngine
 * @param {Record<string, Object>} moduleImports
 * @returns {Promise<Object>}
 */
export async function initEngine(moduleImports = {}) {
    const time = performance.now();
    console.log('[REngine]: Initializing');
    console.log('[REngine]: Loading Modules');
    
    const [dotNetModule, libEngineExports] = await Promise.all([
       dotnet.create(),
       initDriver() 
    ]);
    
    const coreExports = initCore();
    
    const { setModuleImports, getAssemblyExports, getConfig } = dotNetModule;
    setModuleImports('librengine.js', {...libEngineExports});
    setModuleImports('libenginecore.js', {...coreExports});
    
    // Setup External Module Imports
    Object.entries(moduleImports).forEach(pair => {
        const [key, importObj] = pair;
        setModuleImports(key, {...importObj});
    });
    
    console.log('[REngine]: Starting .NET');
    const exports = await getAssemblyExports(getConfig().mainAssemblyName);
    
    console.log('[REngine]: Running .NET');
    await dotnet.run();
    
    const endTime = performance.now();
    console.log(`[REngine]: Engine is Running. Took: ${((endTime - time) / 1000)}ms`);
    
    return exports;
}