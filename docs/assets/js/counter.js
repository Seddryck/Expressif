(function() {
    var counterRoot = document.querySelector("[data-nuget-package]");
    if (!counterRoot) {
        return;
    }

    var packageId = counterRoot.getAttribute("data-nuget-package");
    if (!packageId) {
        return;
    }

    var inlineCountElement = document.getElementById("total-download-count-inline");
    if (!inlineCountElement) {
        return;
    }

    var queryUrl = "https://azuresearch-usnc.nuget.org/query?q=packageid:" + encodeURIComponent(packageId) + "&prerelease=true&semVerLevel=2.0.0&take=1";

    function formatCount(value) {
        return new Intl.NumberFormat("en-US").format(value);
    }

    function setCount(value) {
        var formatted = formatCount(value);
        inlineCountElement.textContent = formatted;
    }

    function setPlaceholder(value) {
        inlineCountElement.textContent = value;
    }

    function getTotalDownloads(packageData) {
        if (!packageData || !Array.isArray(packageData.versions)) {
            return 0;
        }

        return packageData.versions.reduce(function(sum, version) {
            var downloads = Number(version.downloads) || 0;
            return sum + downloads;
        }, 0);
    }

    setPlaceholder("Loading...");

    fetch(queryUrl)
        .then(function(response) {
            if (!response.ok) {
                throw new Error("Failed to load NuGet data");
            }

            return response.json();
        })
        .then(function(data) {
            if (!data || !Array.isArray(data.data)) {
                return;
            }

            var match = data.data.find(function(pkg) {
                return pkg.id && pkg.id.toLowerCase() === packageId.toLowerCase();
            });

            if (!match) {
                return;
            }

            setCount(getTotalDownloads(match));
        })
        .catch(function() {
            setPlaceholder("Unavailable");
        });
})();
