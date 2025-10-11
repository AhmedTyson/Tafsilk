(function(){
  function includePartials(){
    const includeNodes = Array.from(document.querySelectorAll('[data-include]'));
    if (includeNodes.length === 0) {
      window.__partialsLoaded = true;
      document.dispatchEvent(new CustomEvent('partials:loaded'));
      return;
    }

    let remaining = includeNodes.length;
    includeNodes.forEach(function(node){
      const url = node.getAttribute('data-include');
      if (!url) {
        if (--remaining === 0) done();
        return;
      }
      fetch(url, { cache: 'no-cache' })
        .then(function(res){ return res.text(); })
        .then(function(html){
          node.innerHTML = html;
        })
        .catch(function(){
          node.innerHTML = '';
        })
        .finally(function(){
          if (--remaining === 0) done();
        });
    });

    function done(){
      window.__partialsLoaded = true;
      document.dispatchEvent(new CustomEvent('partials:loaded'));
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', includePartials);
  } else {
    includePartials();
  }
})();
