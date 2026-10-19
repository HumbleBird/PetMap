// category.js
export function getPrimaryCategory(category) {
  if (!category) return null;
  if (Array.isArray(category) && category.length) return category[0];
  if (typeof category === 'string') {
    return category.split(/[;,|\/]/)[0].trim();
  }
  return String(category);
}