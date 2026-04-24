export function DescriptionSection({ description }: { description: string }) {
  return (
    <section className="bg-white border rounded" style={{ borderColor: '#d5dbdb' }}>
      <div className="px-4 py-3 border-b" style={{ borderColor: '#d5dbdb', background: '#fafafa' }}>
        <h2 className="text-sm font-semibold" style={{ color: '#16191f' }}>
          Description
        </h2>
      </div>
      <div className="px-4 py-4">
        {description.trim() ? (
          <pre
            className="text-sm whitespace-pre-wrap font-sans leading-relaxed"
            style={{ color: '#16191f' }}
          >
            {description}
          </pre>
        ) : (
          <p className="text-sm italic" style={{ color: '#879596' }}>
            No description provided.
          </p>
        )}
      </div>
    </section>
  );
}
