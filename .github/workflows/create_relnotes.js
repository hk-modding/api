// @ts-check
/** @param {import('github-script').AsyncFunctionArguments} args */
module.exports = async ({ github, core }) => {
    const handlebars = require("handlebars");
    const fs = require("node:fs/promises");
    const crypto = require("node:crypto");

    const { HK_VERSION, API_VERSION, CHANGELOG } = process.env;

    const relnotes = await fs.readFile("./.github/workflows/release_notes.md", { encoding: "utf-8" });
    const template = handlebars.compile(relnotes);

    const artifact_windows = await fs.readFile(`./moddingapi.${API_VERSION}.windows.zip`);
    const artifact_macos = await fs.readFile(`./moddingapi.${API_VERSION}.macos.zip`);
    const artifact_linux = await fs.readFile(`./moddingapi.${API_VERSION}.linux.zip`);

    const hash_windows = crypto.createHash("sha256");
    hash_windows.update(artifact_windows);

    const hash_macos = crypto.createHash("sha256");
    hash_macos.update(artifact_macos);

    const hash_linux = crypto.createHash("sha256");
    hash_linux.update(artifact_linux);

    const output = template({
        api_version: API_VERSION,
        hk_version: HK_VERSION,
        changelog: CHANGELOG,
        windows_cs: hash_windows.digest("hex"),
        macos_cs: hash_macos.digest("hex"),
        linux_cs: hash_linux.digest("hex"),
    });
    
    return output;
};