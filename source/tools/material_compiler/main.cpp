#include <iostream>
#include <fstream>
#include <vector>
#include <string>
#include <filesystem>

#include <slang.h>
#include <slang-com-helper.h>

#include <rengine/rengine.h>

using namespace slang;

namespace
{
SlangStage guessStage(const std::string& path)
{
    auto filename = std::filesystem::path(path).filename().string();
    for (auto& c : filename) c = (char)std::tolower(c);

    if (filename.find("vs") != std::string::npos || filename.find("vert") != std::string::npos)
        return SLANG_STAGE_VERTEX;
    if (filename.find("ps") != std::string::npos || filename.find("frag") != std::string::npos)
        return SLANG_STAGE_FRAGMENT;
    if (filename.find("cs") != std::string::npos || filename.find("comp") != std::string::npos)
        return SLANG_STAGE_COMPUTE;
    return SLANG_STAGE_NONE;
}

}

int main(int argc, char** argv)
{
    if (argc < 2)
    {
        std::cerr << "Usage: material_compiler <shader file>" << std::endl;
        return 1;
    }

    const std::string shaderPath = argv[1];

    std::ifstream file(shaderPath, std::ios::binary);
    if (!file)
    {
        std::cerr << "Failed to open file: " << shaderPath << std::endl;
        return 1;
    }

    std::string source((std::istreambuf_iterator<char>(file)), std::istreambuf_iterator<char>());

    rengine::init();

    SlangSession* session = spCreateSession(nullptr);
    SlangCompileRequest* request = spCreateCompileRequest(session);

    int translationUnit = spAddTranslationUnit(request, SLANG_SOURCE_LANGUAGE_SLANG, "shader");
    spAddTranslationUnitSourceString(request, translationUnit, shaderPath.c_str(), source.c_str());

    SlangStage stage = guessStage(shaderPath);
    if (stage != SLANG_STAGE_NONE)
        spAddEntryPoint(request, translationUnit, "main", stage);

    if (spCompile(request) != SLANG_OK)
    {
        const char* diagnostic = spGetDiagnosticOutput(request);
        if (diagnostic)
            std::cerr << diagnostic << std::endl;
        spDestroyCompileRequest(request);
        spDestroySession(session);
        rengine::destroy();
        return 1;
    }

    std::cout << "Slang compilation succeeded" << std::endl;

    ProgramLayout* program = ProgramLayout::get(request);
    SlangUInt paramCount = program->getParameterCount();

    for (SlangUInt i = 0; i < paramCount; ++i)
    {
        auto* param = program->getParameterByIndex(i);
        auto* var = param->getVariable();
        auto* typeLayout = param->getTypeLayout();
        auto* type = typeLayout->getType();

        auto kind = type->getKind();
        auto category = param->getCategory();

        if (kind == TypeReflection::Kind::ConstantBuffer)
        {
            std::cout << "ConstantBuffer: " << var->getName() << std::endl;
        }
        else if (category == ParameterCategory::VaryingInput)
        {
            const char* semantic = param->getSemanticName();
            std::cout << "VertexInput: " << var->getName();
            if (semantic)
                std::cout << " (" << semantic << param->getSemanticIndex() << ")";
            std::cout << std::endl;
        }
        else if (typeLayout->getBindingRangeCount() > 0)
        {
            for (SlangInt r = 0; r < typeLayout->getBindingRangeCount(); ++r)
            {
                auto bindingType = typeLayout->getBindingRangeType(r);
                if (bindingType == BindingType::Texture)
                {
                    std::cout << "Texture: " << var->getName() << std::endl;
                }
            }
        }
    }

    spDestroyCompileRequest(request);
    spDestroySession(session);
    rengine::destroy();
    return 0;
}

